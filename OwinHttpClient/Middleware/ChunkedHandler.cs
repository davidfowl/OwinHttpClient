using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin.Http;
using Owin.Types;

namespace Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ChunkedHandler
    {
        private readonly AppFunc _next;

        public ChunkedHandler(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _next(environment);

            var response = new OwinResponse(environment);
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
