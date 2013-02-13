using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;
using Owin.Types;

namespace Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class GzipHandler
    {
        private readonly AppFunc _next;

        public GzipHandler(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _next(environment);

            var response = new OwinResponse(environment);
            var encoding = response.GetHeader("Content-Encoding");

            if (String.Equals(encoding, "gzip", StringComparison.OrdinalIgnoreCase))
            {
                response.Body = new GZipStream(response.Body, CompressionMode.Decompress);
            }
        }
    }
}
