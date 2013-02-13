using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin.Http;
using Owin.Types;

namespace Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ContentLengthHandler
    {
        private readonly AppFunc _next;

        public ContentLengthHandler(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _next(environment);

            var response = new OwinResponse(environment);
            string contentLengthRaw = response.GetHeader("Content-Length");
            long contentLength;

            if (contentLengthRaw != null && 
                Int64.TryParse(contentLengthRaw, out contentLength))
            {
                response.Body = new ContentLengthStream(response.Body, contentLength);
            }
        }
    }
}
