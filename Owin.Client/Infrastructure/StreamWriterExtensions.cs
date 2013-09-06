using System.IO;

namespace Owin.Client.Infrastructure
{
    public static class StreamWriterExtensions
    {
        static string HTTP_NEW_LINE = "\r\n";

        public static void WriteHttpLine(this StreamWriter writer)
        {
            writer.Write(HTTP_NEW_LINE);
        }

        public static void WriteHttpLine(this StreamWriter writer, string format, object arg0, object arg1)
        {
            writer.Write(format + HTTP_NEW_LINE, arg0, arg1);
        }

        public static void WriteHttpLine(this StreamWriter writer, string format, object arg0, object arg1, object arg2)
        {
            writer.Write(format + HTTP_NEW_LINE, arg0, arg1, arg2);
        }
    }
}
