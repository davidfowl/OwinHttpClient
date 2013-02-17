using System.Collections.Concurrent;
using System.IO;
using Owin.Client;
using Xunit;

namespace Owin.Tests
{
    public class RequestBuilderFacts
    {
        [Fact]
        public void BuildRequestFromUriTest()
        {
            var requestHeaders = new ConcurrentDictionary<string, string[]>();
            requestHeaders["Host"] = new string[] { "localhost:80" };
            var expected = new ConcurrentDictionary<string, object>();
            expected["owin.RequestHeaders"] = requestHeaders;
            expected["owin.ResponseBody"] = new MemoryStream();
            expected["owin.RequestProtocol"] = "HTTP/1.1";
            expected["owin.ResponseHeaders"] = new ConcurrentDictionary<string, string[]>();
            expected["owin.RequestQueryString"] = "";
            expected["owin.RequestScheme"] = "http";
            expected["owin.RequestPath"] = "/";
            expected["owin.RequestPathBase"] = "";
            expected["owin.RequestMethod"] = "GET";

            var actual = RequestBuilder.Get("http://localhost/");

            Assert.Equal(expected["owin.RequestProtocol"],
                actual["owin.RequestProtocol"]);

            Assert.Equal(expected["owin.RequestQueryString"],
                actual["owin.RequestQueryString"]);

            Assert.Equal(expected["owin.RequestScheme"],
                actual["owin.RequestScheme"]);

            Assert.Equal(expected["owin.RequestPath"],
                actual["owin.RequestPath"]);

            Assert.Equal(expected["owin.RequestPathBase"],
                actual["owin.RequestPathBase"]);

            Assert.Equal(expected["owin.RequestMethod"],
                actual["owin.RequestMethod"]);
        }
    }
}
