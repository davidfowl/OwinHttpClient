using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Owin.Types;

namespace Owin
{
    public static class OwinHttpClientExtensions
    {
        public static async Task<IDictionary<string, object>> Get(this OwinHttpClient client, string url)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            OwinRequest request = CreateRequest(url, "GET");
            await client.Invoke(request.Dictionary);
            return request.Dictionary;
        }

        public static Task<IDictionary<string, object>> Post(this OwinHttpClient client, string url, IDictionary<string, string> postData)
        {
            return client.Post(url,
                               "application/x-www-form-urlencoded; charset=UTF-8",
                               GetRequestBody(postData));
        }

        public static async Task<IDictionary<string, object>> Post(this OwinHttpClient client, string url, string contentType, Stream stream)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            OwinRequest request = CreateRequest(url, "POST");

            request.SetHeader("Content-Type", contentType);
            request.Body = stream;
            request.SetHeader("Content-Length", stream.Length.ToString());

            await client.Invoke(request.Dictionary);
            return request.Dictionary;
        }

        private static OwinRequest CreateRequest(string url, string method)
        {
            var uri = new Uri(url);
            var request = OwinRequest.Create();
            request.Protocol = "HTTP/1.1";
            request.Headers = new Dictionary<string, string[]>();
            request.Method = method;
            request.Host = uri.Host;
            request.PathBase = String.Empty;
            request.Path = uri.LocalPath;
            request.Scheme = uri.Scheme;
            request.QueryString = uri.Query.Length > 0 ? uri.Query.Substring(1) : String.Empty;
            return request;
        }

        private static Stream GetRequestBody(IDictionary<string, string> postData)
        {
            var ms = new MemoryStream();
            if (postData != null)
            {
                bool first = true;
                var writer = new StreamWriter(ms);
                writer.AutoFlush = true;
                foreach (var item in postData)
                {
                    if (!first)
                    {
                        writer.Write("&");
                    }
                    writer.Write(item.Key);
                    writer.Write("=");
                    writer.Write(Uri.EscapeDataString(item.Value));
                    writer.WriteLine();
                    first = false;
                }

                ms.Seek(0, SeekOrigin.Begin);
            }
            return ms;
        }
    }
}
