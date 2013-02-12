using System;
using System.IO;

namespace Owin
{
    internal static class HttpParser
    {
        public static void ParseRequest(Stream stream, Action<string, string, string> onRequestLine, Action<string, string> onHeader)
        {
            string method;
            string path;
            string protocol;
            Parse3Tokens(stream, out method, out path, out protocol);
            onRequestLine(method, path, protocol);

            ParseHeaders(stream, onHeader);
        }

        public static void ParseResponse(Stream stream, Action<string, int, string> onResponseLine, Action<string, string> onHeader)
        {
            string protocol;
            string statusCode;
            string reasonPhrase;
            Parse3Tokens(stream, out protocol, out statusCode, out reasonPhrase);
            onResponseLine(protocol, Int32.Parse(statusCode), reasonPhrase);

            ParseHeaders(stream, onHeader);
        }

        private static void Parse3Tokens(Stream stream, out string token1, out string token2, out string token3)
        {
            var reader = new StringReader(stream.ReadLine());
            token1 = reader.ReadUntilWhitespace();
            reader.SkipWhitespace();
            token2 = reader.ReadUntilWhitespace();
            reader.SkipWhitespace();
            token3 = reader.ReadLine();
        }

        private static void ParseHeaders(Stream stream, Action<string, string> onHeader)
        {
            string headerLine = null;

            while (true)
            {
                headerLine = stream.ReadLine();

                if (headerLine == String.Empty)
                {
                    break;
                }

                var headerReader = new StringReader(headerLine);
                string key = headerReader.ReadUntil(c => c == ':');
                headerReader.Read();
                headerReader.SkipWhitespace();
                string value = headerReader.ReadToEnd();

                onHeader(key, value);
            }
        }
    }
}
