using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owin
{
    class Program
    {
        static void Main(string[] args)
        {
            MakeRequest().Wait();
        }

        private static async Task MakeRequest()
        {
            var client = new OwinHttpClient();
            var env = await client.Get("http://httpbin.org/status/418");


            Console.WriteLine("========");
            Console.WriteLine("Request");
            Console.WriteLine("========");
            Console.WriteLine(env[OwinHttpClientConstants.HttpClientRawRequest]);

            Console.WriteLine("========");
            Console.WriteLine("Response");
            Console.WriteLine("========");
            Console.WriteLine(env[OwinHttpClientConstants.HttpClientRawResponse]);

            var reader = new StreamReader((Stream)env["owin.ResponseBody"]);
            Console.WriteLine(await reader.ReadToEndAsync());
        }
    }
}
