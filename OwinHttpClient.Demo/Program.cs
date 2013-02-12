using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Owin
{
    class Program
    {
        static void Main(string[] args)
        {
            MakeRequest(200).Wait();
            FollowRedirects(5).Wait();
            MakeRawRequest().Wait();
        }

        private static async Task MakeRawRequest()
        {
            string rawRequest =
@"GET http://www.reddit.com/ HTTP/1.1
Host: www.reddit.com
Connection: keep-alive
Cache-Control: max-age=0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
User-Agent: Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.57 Safari/537.17
Accept-Encoding: gzip,deflate,sdch
Accept-Language: en-US,en;q=0.8
Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3

";

            var client = new OwinHttpClient();
            var env = Request.FromRaw(rawRequest);

            await client.Invoke(env);

            Console.WriteLine("========");
            Console.WriteLine("Request");
            Console.WriteLine("========");
            Console.WriteLine(env[OwinHttpClientConstants.HttpClientRawRequest]);

            await PrintResponse(env);
        }

        private static async Task MakeRequest(int statusCode)
        {
            var client = new OwinHttpClient();
            var env = Request.Get("http://httpbin.org/status/" + statusCode);
            await client.Invoke(env);

            Console.WriteLine("========");
            Console.WriteLine("Request");
            Console.WriteLine("========");
            Console.WriteLine(env[OwinHttpClientConstants.HttpClientRawRequest]);

            await PrintResponse(env);
        }

        private static async Task PrintResponse(IDictionary<string, object> env)
        {
            Console.WriteLine("========");
            Console.WriteLine("Response");
            Console.WriteLine("========");
            Console.WriteLine(env[OwinHttpClientConstants.HttpClientRawResponse]);

            var reader = new StreamReader((Stream)env["owin.ResponseBody"]);
            Console.WriteLine(await reader.ReadToEndAsync());
        }

        private static async Task FollowRedirects(int n)
        {
            var client = new OwinHttpClient();

            string url = "http://httpbin.org/redirect/" + n;

            IDictionary<string, object> env;

            while (true)
            {
                env = Request.Get(url);

                await client.Invoke(env);
                
                Console.WriteLine("========");
                Console.WriteLine("Request");
                Console.WriteLine("========");
                Console.WriteLine(env[OwinHttpClientConstants.HttpClientRawRequest]);

                await PrintResponse(env);

                var statusCode = (int)env["owin.ResponseStatusCode"];
                if (statusCode == 302 || statusCode == 301)
                {
                    var headers = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
                    url = headers["Location"][0];
                }
                else
                {
                    break;
                }
            }            
        }
    }
}
