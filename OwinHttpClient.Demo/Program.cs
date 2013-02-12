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
        }

        private static async Task MakeRequest(int statusCode)
        {
            var client = new OwinHttpClient();
            var env = client.Get("http://httpbin.org/status/" + statusCode);
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
                env = client.Get(url);

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
