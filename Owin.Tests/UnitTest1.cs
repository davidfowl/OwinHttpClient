using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin.Client;
using System.Collections.Concurrent;
using System.IO;

namespace Owin.Tests
{
    [TestClass]
    public class RequestBuilderTest
    {
        [TestMethod]
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

            Assert.AreEqual(expected["owin.RequestProtocol"],
                actual["owin.RequestProtocol"]);

            Assert.AreEqual(expected["owin.RequestQueryString"],
                actual["owin.RequestQueryString"]);

            Assert.AreEqual(expected["owin.RequestScheme"],
                actual["owin.RequestScheme"]);

            Assert.AreEqual(expected["owin.RequestPath"],
                actual["owin.RequestPath"]);

            Assert.AreEqual(expected["owin.RequestPathBase"],
                actual["owin.RequestPathBase"]);

            Assert.AreEqual(expected["owin.RequestMethod"],
                actual["owin.RequestMethod"]);


        }
    }
}
