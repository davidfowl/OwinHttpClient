using System;
using System.IO;
using System.Text;

namespace Owin
{
    internal static class StreamExtensions
    {
        public static string ReadLine(this Stream ns)
        {
            var builder = new StringBuilder();

            byte cur = 0;
            byte prev = 0;

            while (true)
            {
                if (cur == 10)
                {
                    if (prev == 13)
                    {
                        // Remove /r/n
                        builder.Remove(builder.Length - 2, 2);
                    }
                    else
                    {
                        // Remove /n
                        builder.Remove(builder.Length - 1, 1);
                    }
                    break;
                }

                prev = cur;
                cur = (byte)ns.ReadByte();

                builder.Append((char)cur);
            }

            return builder.ToString();
        }

    }
}
