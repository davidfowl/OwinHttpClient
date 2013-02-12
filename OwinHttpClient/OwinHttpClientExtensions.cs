using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin.Types;

namespace Owin
{
    public static class OwinHttpClientExtensions
    {
        public static IDictionary<string, object> FromRaw(this OwinHttpClient client, string rawRequest)
        {
            var request = OwinRequest.Create();

            var reader = new StringReader(rawRequest);
            request.Method = reader.ReadUntilWhitespace();
            reader.SkipWhitespace();
            var uri = new Uri(reader.ReadUntilWhitespace());
            reader.SkipWhitespace();
            request.Protocol = reader.ReadLine();
            request = BuildRequestFromUri(uri, request);

            string headerLine = null;

            while (true)
            {
                headerLine = reader.ReadLine();

                if (headerLine == String.Empty)
                {
                    break;
                }

                var headerReader = new StringReader(headerLine);
                string key = headerReader.ReadUntil(c => c == ':');
                headerReader.Read();
                headerReader.SkipWhitespace();
                string value = headerReader.ReadToEnd();

                request.SetHeader(key, value);
            }

            var body = reader.ReadToEnd();

            if (String.IsNullOrEmpty(body))
            {
                request.Body = Stream.Null;
            }
            else
            {
                request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
            }

            return request.Dictionary;
        }

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
            request.Method = method;
            request = BuildRequestFromUri(uri, request);
            return request;
        }

        private static OwinRequest BuildRequestFromUri(Uri uri, OwinRequest request)
        {
            request.Host = uri.GetComponents(UriComponents.HostAndPort, UriFormat.Unescaped);
            request.PathBase = String.Empty;
            request.Path = uri.LocalPath;
            request.Scheme = uri.Scheme;
            request.QueryString = uri.Query.Length > 0 ? uri.Query.Substring(1) : String.Empty;
            return request;
        }
    }
}
