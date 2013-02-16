using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Owin.Client;
using Owin.Types;
using Owin;
using Nancy;
using System.Web.Http;

namespace OwinDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Make SignalR request in memory");
            MakeSignalRRequest().Wait();
            Console.ReadKey();

            Console.WriteLine("Make Web API request in memory");
            MakeWebApiRequest().Wait();
            Console.ReadKey();

            Console.WriteLine("Make Nancy request in memory");
            MakeNancyRequest().Wait();
            Console.ReadKey();

            Console.WriteLine("Make https request");
            MakeHttpsRequest().Wait();
            Console.ReadKey();

            Console.WriteLine("Make authenticated request");
            MakeBasicAuthRequest().Wait();
            Console.ReadKey();

            Console.WriteLine("Make 204 request");
            MakeRequest(204).Wait();
            Console.ReadKey();

            Console.WriteLine("Following 3 redirects");
            FollowRedirects(3).Wait();
            Console.ReadKey();

            Console.WriteLine("Make request from raw request body");
            MakeRawRequest().Wait();
            Console.ReadKey();

            Console.WriteLine("Make request to gzipped resource");
            MakeGzippedRequest().Wait();
            Console.ReadKey();

            Console.WriteLine("Make chunked request");
            MakeChunkedRequest().Wait();
        }

        private static async Task MakeWebApiRequest() 
        {
            var client = new OwinHttpClient(app => 
            {
                var config = new HttpConfiguration();
                config.Routes.MapHttpRoute("DefaultRoute", "{controller}");
                app.UseHttpServer(config);
            });

            var webApiEnv = RequestBuilder.Get("http://foo/cars");
            await client.Invoke(webApiEnv);
            await PrintResponse(webApiEnv);
        }

        private static async Task MakeNancyRequest()
        {
            var client = new OwinHttpClient(app =>
            {
                app.UseNancy();
            });

            var nancyEnv = RequestBuilder.Get("http://foo/hello");

            await client.Invoke(nancyEnv);

            await PrintResponse(nancyEnv);
        }

        private static async Task MakeSignalRRequest()
        {
            var client = new OwinHttpClient(app =>
            {
                app.MapHubs();
            });

            var env = RequestBuilder.Get("http://foo/signalr/hubs");

            await client.Invoke(env);

            await PrintResponse(env);
        }

        private static async Task MakeChunkedRequest()
        {
            var client = new OwinHttpClient();
            var env = RequestBuilder.Get("http://www.google.com/");
            await client.Invoke(env);

            await PrintResponse(env);
        }

        private static async Task MakeHttpsRequest()
        {
            var client = new OwinHttpClient();
            var env = RequestBuilder.Get("https://www.google.com/");
            await client.Invoke(env);

            await PrintResponse(env);
        }

        private static async Task MakeBasicAuthRequest()
        {
            var client = new OwinHttpClient();
            var env = RequestBuilder.Get("http://www.httpbin.org/basic-auth/john/doe");

            await client.Invoke(env);

            var response = new OwinResponse(env);
            if (response.StatusCode != 401)
            {
                return;
            }

            env = RequestBuilder.Get("http://www.httpbin.org/basic-auth/john/doe")
                                .WithCredentials("john", "doe");

            await client.Invoke(env);

            await PrintResponse(env);
        }

        private static async Task MakeGzippedRequest()
        {
            var client = new OwinHttpClient();

            var env = RequestBuilder.Get("http://www.httpbin.org/gzip");
            await client.Invoke(env);

            await PrintResponse(env);
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

            var env = RequestBuilder.FromRaw(rawRequest);

            await client.Invoke(env);

            await PrintResponse(env);
        }

        private static async Task MakeRequest(int statusCode)
        {
            var client = new OwinHttpClient();
            var env = RequestBuilder.Get("http://httpbin.org/status/" + statusCode);
            await client.Invoke(env);

            await PrintResponse(env);
        }

        private static async Task PrintResponse(IDictionary<string, object> env)
        {
            var response = new OwinResponse(env);
            var reader = new StreamReader(response.Body);
            Console.WriteLine(await reader.ReadToEndAsync());
        }

        private static async Task FollowRedirects(int n)
        {
            var client = new OwinHttpClient();

            string url = "http://httpbin.org/redirect/" + n;

            var env = RequestBuilder.Get(url);

            await client.Invoke(env);

            await PrintResponse(env);
        }
    }

    public class MyModule : NancyModule
    {
        public MyModule()
        {
            Get["/hello"] = _ =>
            {
                return "Hello From Nancy";
            };
        }
    }

    public class CarsController : ApiController 
    {
        public string[] GetCars() 
        {
            return new[] 
            { 
                "Car 1",
                "Car 2",
                "Car 3"
            };
        }
    }
}
