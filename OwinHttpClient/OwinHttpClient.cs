using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Owin.Types;

namespace Owin
{
    public class OwinHttpClient
    {
        private readonly IStreamFactory _streamFactory;

        public OwinHttpClient()
            : this(new NetworkStreamFactory())
        {
        }

        public OwinHttpClient(IStreamFactory streamFactory)
        {
            _streamFactory = streamFactory;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var request = new OwinRequest(environment);
            Uri uri = request.Uri;

            // Create a stream for the host and port so we can send the request
            Stream stream = await _streamFactory.CreateStream(uri.Host, uri.Port).ConfigureAwait(continueOnCapturedContext: false);

            var requestWriter = new StreamWriter(stream);

            // Request line
            requestWriter.WriteLine("{0} {1} {2}", request.Method, uri.LocalPath, request.Protocol);

            // Write headers
            foreach (var header in request.Headers)
            {
                requestWriter.WriteLine("{0}: {1}", header.Key, request.GetHeader(header.Key));
            }

            // End headers
            requestWriter.WriteLine();

            if (request.Body == null)
            {
                // End request
                requestWriter.WriteLine();
            }

            // Flush buffered content to the stream async
            await requestWriter.FlushAsync().ConfigureAwait(continueOnCapturedContext: true);

            if (request.Body != null)
            {
                // Copy the body to the request
                await request.Body.CopyToAsync(stream).ConfigureAwait(continueOnCapturedContext: false);
            }

            // Populate the response
            await ReadResponse(stream, environment).ConfigureAwait(continueOnCapturedContext: false);
        }

        private static async Task ReadResponse(Stream stream, IDictionary<string, object> environment)
        {
            var response = new OwinResponse(environment);

            HttpParser.ParseResponse(stream, (protocol, statusCode, reasonPhrase) =>
            {
                response.Protocol = protocol;
                response.StatusCode = statusCode;
                response.ReasonPhrase = reasonPhrase;
            },
            (key, value) => response.SetHeader(key, value));

            var request = new OwinRequest(environment);

            Stream responseBody = null;

            // 100-199, 204, 304 and HEAD requests never have a response body
            if ((response.StatusCode >= 100 && response.StatusCode <= 199) ||
                response.StatusCode == 204 ||
                response.StatusCode == 304 ||
                request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
            {
                responseBody = Stream.Null;
            }
            else
            {
                var transferEncoding = response.GetHeader("Transfer-Encoding");

                if (transferEncoding != null &&
                    transferEncoding.Equals("chunked", StringComparison.OrdinalIgnoreCase))
                {
                    responseBody = new ChunkedStream(stream);
                }
                else
                {
                    int contentLength = Int32.Parse(response.GetHeader("Content-Length"));
                    responseBody = new ContentLengthStream(stream, contentLength);
                }
            }

            string connection = response.GetHeader("Connection");

            if (responseBody == null ||
                (response.Protocol.Equals("HTTP/1.1", StringComparison.OrdinalIgnoreCase) &&
                String.Equals(connection, "Close", StringComparison.OrdinalIgnoreCase)))
            {
                var ms = new MemoryStream();

                using (stream)
                {
                    await (responseBody ?? stream).CopyToAsync(ms).ConfigureAwait(continueOnCapturedContext: false);
                    ms.Seek(0, SeekOrigin.Begin);
                    responseBody = ms;
                }
            }

            response.Body = responseBody;
        }
    }
}
