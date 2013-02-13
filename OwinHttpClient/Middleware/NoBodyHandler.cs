using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Owin.Types;

namespace Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class NoBodyHandler
    {
        private readonly AppFunc _next;

        public NoBodyHandler(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _next(environment);

            var request = new OwinRequest(environment);
            var response = new OwinResponse(environment);

            // 100-199, 204, 304 and HEAD requests never have a response body
            if ((response.StatusCode >= 100 && response.StatusCode <= 199) ||
                response.StatusCode == 204 ||
                response.StatusCode == 304 ||
                request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
            {
                // Close the existing body
                response.Body.Close();

                response.Body = Stream.Null;
            }
        }
    }
}
