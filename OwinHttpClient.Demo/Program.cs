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
            var env = await client.Get("http://www.google.com/");

            var reader = new StreamReader((Stream)env["owin.ResponseBody"]);
            Console.WriteLine(await reader.ReadToEndAsync());
        }
    }
}
