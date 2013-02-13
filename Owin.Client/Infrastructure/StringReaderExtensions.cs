using System;
using System.IO;
using System.Text;

namespace Owin.Client.Infrastructure
{
    internal static class StringReaderExtensions
    {
        public static void SkipWhitespace(this StringReader reader)
        {
            ReadUntil(reader, c => !Char.IsWhiteSpace(c));
        }

        public static string ReadUntilWhitespace(this StringReader reader)
        {
            return ReadUntil(reader, c => Char.IsWhiteSpace(c));
        }

        public static string ReadToEnd(this StringReader reader)
        {
            return ReadUntil(reader, c => false);
        }

        public static string ReadUntil(this StringReader reader, Func<char, bool> predicate)
        {
            var sb = new StringBuilder();
            int ch = -1;
            do
            {
                ch = reader.Peek();
                if (ch == -1 || predicate((char)ch))
                {
                    break;
                }
                sb.Append((char)ch);
                reader.Read();
            }
            while (true);
            return sb.ToString();
        }
    }
}
