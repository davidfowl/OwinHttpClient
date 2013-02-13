using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Owin.Http;
using Owin.Types;

namespace Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class HttpResponseHandler
    {
        private readonly AppFunc _next;

        public HttpResponseHandler(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _next(environment);

            var request = new OwinRequest(environment);
            var response = new OwinResponse(environment);

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
                    responseBody = new ChunkedStream(response.Body);
                }
                else
                {
                    int contentLength = Int32.Parse(response.GetHeader("Content-Length"));
                    responseBody = new ContentLengthStream(response.Body, contentLength);
                }
            }

            string connection = response.GetHeader("Connection");

            if (responseBody == null ||
                (response.Protocol.Equals("HTTP/1.1", StringComparison.OrdinalIgnoreCase) &&
                String.Equals(connection, "Close", StringComparison.OrdinalIgnoreCase)))
            {
                var ms = new MemoryStream();

                using (response.Body)
                {
                    await (responseBody ?? response.Body).CopyToAsync(ms).ConfigureAwait(continueOnCapturedContext: false);
                    ms.Seek(0, SeekOrigin.Begin);
                    responseBody = ms;
                }
            }

            response.Body = responseBody;
        }
    }
}
