using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Owin.Http;
using Owin.Types;

namespace Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ContentLengthMiddleware
    {
        private readonly AppFunc _next;

        public ContentLengthMiddleware(AppFunc next)
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
