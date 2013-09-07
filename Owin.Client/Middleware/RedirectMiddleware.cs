using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin.Types;

namespace Owin.Client.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RedirectMiddleware
    {
        private readonly AppFunc _next;
        private readonly int _maxRedirects;

        public RedirectMiddleware(AppFunc next, int maxRedirects)
        {
            _next = next;
            _maxRedirects = maxRedirects;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            int maxRedirects = _maxRedirects;

            while (maxRedirects >= 0)
            {
                await _next(environment);

                var response = new OwinResponse(environment);

                if (response.StatusCode == 302 || response.StatusCode == 301)
                {
                    string url = BuildRedirectUrl(response);

                    // Clear the env so we can make a new request
                    environment.Clear();

                    // Populate the env with new request data
                    RequestBuilder.BuildGet(environment, url);
                }
                else
                {
                    break;
                }

                maxRedirects--;
            }
        }

        private string BuildRedirectUrl(OwinResponse response)
        {
            string location = response.GetHeader("Location");

            Uri uri;
            if (Uri.TryCreate(location, UriKind.Relative, out uri))
            {
                // If location is relative, we need to build a full url
                var previousRequest = new OwinRequest(response.Dictionary);
                var uriBuilder = new UriBuilder(previousRequest.Uri);
                uriBuilder.Path = location;

                return uriBuilder.ToString();
            }

            return location;
        }
    }
}
