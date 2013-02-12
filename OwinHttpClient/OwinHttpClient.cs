using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Owin.Types;

namespace Owin
{
    public class OwinHttpClient
    {
        private readonly IStreamFactory _streamFactory;

        public OwinHttpClient()
            : this(new DefaultStreamFactory())
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

            var requestBuilder = new StringBuilder();
            request.Dictionary[OwinHttpClientConstants.HttpClientRawRequest] = requestBuilder;

            // Request line
            requestBuilder.AppendFormat("{0} {1} {2}", request.Method, uri.LocalPath, request.Protocol).AppendLine();

            // Write headers
            foreach (var header in request.Headers)
            {
                requestBuilder.Append(header.Key)
                              .Append(": ")
                              .Append(request.GetHeader(header.Key))
                              .AppendLine();
            }

            // End headers
            requestBuilder.AppendLine();

            if (request.Body == null)
            {
                // End request
                requestBuilder.AppendLine();
            }

            byte[] requestBuffer = Encoding.UTF8.GetBytes(requestBuilder.ToString());
            var ms = new MemoryStream(requestBuffer, 0, requestBuffer.Length, writable: true, publiclyVisible: true);

            if (request.Body != null)
            {
                // Copy the body to the request
                await request.Body.CopyToAsync(ms).ConfigureAwait(continueOnCapturedContext: false);
            }

            // Create a stream for the host and port so we can send the request
            Stream stream = await _streamFactory.CreateStream(uri.Host, uri.Port).ConfigureAwait(continueOnCapturedContext: false);

            // Write to the stream async
            await ms.CopyToAsync(stream).ConfigureAwait(continueOnCapturedContext: false);

            // Populate the response
            await ReadResponse(stream, environment).ConfigureAwait(continueOnCapturedContext: false);
        }

        private static async Task ReadResponse(Stream stream, IDictionary<string, object> environment)
        {
            var response = new OwinResponse(environment);

            var responseBuilder = new StringBuilder();
            response.Dictionary[OwinHttpClientConstants.HttpClientRawResponse] = responseBuilder;

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
                responseBody = new ContentLengthStream(stream, 0);
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
                "Close".Equals(connection, StringComparison.OrdinalIgnoreCase)))
            {
                var ms = new MemoryStream();

                if (responseBody == null)
                {
                    await stream.CopyToAsync(ms);
                }
                else
                {
                    await responseBody.CopyToAsync(ms);
                }

                ms.Seek(0, SeekOrigin.Begin);
                responseBody = ms;
                stream.Close();
            }

            response.Body = responseBody;
        }
    }
}
