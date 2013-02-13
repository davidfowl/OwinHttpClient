using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Owin.Client.Http;
using Owin.Types;

namespace Owin.Client.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ChunkedMiddleware
    {
        private readonly AppFunc _next;

        public ChunkedMiddleware(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _next(environment);

            var response = new OwinResponse(environment);

            if (response.Body == Stream.Null)
            {
                return;
            }

            var transferEncoding = response.GetHeader("Transfer-Encoding");

            if (String.Equals(transferEncoding, 
                              "chunked", 
                              StringComparison.OrdinalIgnoreCase))
            {
                response.Body = new ChunkedStream(response.Body);
            }
        }
    }
}
