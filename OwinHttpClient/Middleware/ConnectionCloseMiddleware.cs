using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Owin.Types;

namespace Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ConnectionCloseMiddleware
    {
        private readonly AppFunc _next;

        public ConnectionCloseMiddleware(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _next(environment);

            var response = new OwinResponse(environment);
            string connection = response.GetHeader("Connection");

            if (response.Protocol.Equals("HTTP/1.1", StringComparison.OrdinalIgnoreCase) && 
                String.Equals(connection, "Close", StringComparison.OrdinalIgnoreCase))
            {
                var ms = new MemoryStream();

                using (response.Body)
                {
                    await response.Body.CopyToAsync(ms).ConfigureAwait(continueOnCapturedContext: false);
                }

                ms.Seek(0, SeekOrigin.Begin);
                response.Body = ms;
            }
        }
    }
}
