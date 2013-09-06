using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Owin.Client.Http;
using Owin.Client.Infrastructure;
using Owin.Types;

namespace Owin.Client.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HttpRequestMiddleware
    {
        private readonly AppFunc _next;
        private readonly IStreamFactory _streamFactory;

        public HttpRequestMiddleware(AppFunc next, IStreamFactory streamFactory)
        {
            _next = next;
            _streamFactory = streamFactory;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var request = new OwinRequest(environment);
            Uri uri = request.Uri;

            // Create a stream for the host and port so we can send the request
            Stream stream = await _streamFactory.CreateStream(uri).ConfigureAwait(continueOnCapturedContext: false);

            var requestWriter = new StreamWriter(stream);

            // Request line
            requestWriter.WriteHttpLine("{0} {1} {2}", request.Method, uri.PathAndQuery, request.Protocol);

            // Write headers
            foreach (var header in request.Headers)
            {
                requestWriter.WriteHttpLine("{0}: {1}", header.Key, request.GetHeader(header.Key));
            }

            // End headers
            requestWriter.WriteHttpLine();

            if (request.Body == null)
            {
                // End request
                requestWriter.WriteHttpLine();
            }

            // Flush buffered content to the stream async
            await requestWriter.FlushAsync().ConfigureAwait(continueOnCapturedContext: false);

            if (request.Body != null)
            {
                // Copy the body to the request
                await request.Body.CopyToAsync(stream).ConfigureAwait(continueOnCapturedContext: false);
            }

            var response = new OwinResponse(environment);

            // Parse the response
            HttpParser.ParseResponse(stream, (protocol, statusCode, reasonPhrase) =>
            {
                response.Protocol = protocol;
                response.StatusCode = statusCode;
                response.ReasonPhrase = reasonPhrase;
            },
            (key, value) => response.SetHeader(key, value));

            // Set the body to the rest of the stream
            response.Body = stream;
        }
    }
}
