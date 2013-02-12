using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin.Types;

namespace Owin
{
    public static class OwinHttpClientExtensions
    {
        public static IDictionary<string, object> Get(this OwinHttpClient client, string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            OwinRequest request = CreateRequest(url, "GET");
            return request.Dictionary;
        }

        public static IDictionary<string, object> Post(this OwinHttpClient client, string url)
        {
            OwinRequest request = CreateRequest(url, "POST");
            return request.Dictionary;
        }

        private static OwinRequest CreateRequest(string url, string method)
        {
            var uri = new Uri(url);
            var request = OwinRequest.Create();
            request.Protocol = "HTTP/1.1";
            request.Headers = new Dictionary<string, string[]>();
            request.Method = method;
            request.Host = uri.GetComponents(UriComponents.HostAndPort, UriFormat.Unescaped);
            request.PathBase = String.Empty;
            request.Path = uri.LocalPath;
            request.Scheme = uri.Scheme;
            request.QueryString = uri.Query.Length > 0 ? uri.Query.Substring(1) : String.Empty;
            return request;
        }
    }
}
