using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Owin.Types;

namespace Owin
{
    public class OwinHttpClient
    {
        public async Task Invoke(IDictionary<string, object> environment)
        {
            var request = new OwinRequest(environment);
            Socket socket = ConnectSocket(request.Uri.Host, request.Uri.Port);

            var requestBuilder = new StringBuilder();
            request.Dictionary[OwinHttpClientConstants.HttpClientRawRequest] = requestBuilder;

            // Request line
            requestBuilder.AppendFormat("{0} {1} {2}", request.Method, request.Uri, request.Protocol).AppendLine();

            // Write headers
            foreach (var header in request.Headers)
            {
                requestBuilder.Append(header.Key)
                              .Append(": ")
                              .Append(request.GetHeader(header.Key))
                              .AppendLine();
            }

            // End headers
            requestBuilder.AppendLine();

            if (request.Body == null)
            {
                // End request
                requestBuilder.AppendLine();
            }

            byte[] requestBuffer = Encoding.UTF8.GetBytes(requestBuilder.ToString());
            var ms = new MemoryStream(requestBuffer, 0, requestBuffer.Length, writable: true, publiclyVisible: true);

            if (request.Body != null)
            {
                // Copy the body to the request
                await request.Body.CopyToAsync(ms);
            }

            var tcs = new TaskCompletionSource<object>();

            IAsyncResult result = socket.BeginSend(ms.GetBuffer(), 0, (int)ms.Length, SocketFlags.None, ar =>
            {
                if (ar.CompletedSynchronously)
                {
                    return;
                }

                CompleteSend(ar, socket, tcs, environment);
            },
            null);

            if (result.CompletedSynchronously)
            {
                CompleteSend(result, socket, tcs, environment);
            }

            await tcs.Task;
        }

        private static void CompleteSend(IAsyncResult asyncResult, Socket socket, TaskCompletionSource<object> tcs, IDictionary<string, object> env)
        {
            try
            {
                SocketError error;
                socket.EndSend(asyncResult, out error);
                ReadResponse(socket, env);
                tcs.TrySetResult(null);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        private static Socket ConnectSocket(string host, int port)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(host);

            foreach (IPAddress address in hostEntry.AddressList)
            {
                var ipe = new IPEndPoint(address, port);
                var socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.Connect(ipe);

                if (socket.Connected)
                {
                    return socket;
                }
            }

            return null;
        }

        private static void ReadResponse(Socket socket, IDictionary<string, object> env)
        {
            var stream = new NetworkStream(socket);
            var response = new OwinResponse(env);
            var responseBuilder = new StringBuilder();

            response.Headers = new Dictionary<string, string[]>();
            response.Dictionary[typeof(Socket).FullName] = socket;
            response.Dictionary[OwinHttpClientConstants.HttpClientRawResponse] = responseBuilder;

            string responseLine = stream.ReadLine();
            responseBuilder.AppendLine(responseLine);

            var responseLineReader = new StringReader(responseLine);
            string protocol = responseLineReader.ReadUntilWhitespace(); 
            responseLineReader.SkipWhitespace();
            response.StatusCode = Int32.Parse(responseLineReader.ReadUntilWhitespace());
            responseLineReader.SkipWhitespace();
            response.ReasonPhrase = responseLineReader.ReadToEnd();

            string headerLine = null;

            while (true)
            {
                headerLine = stream.ReadLine();
                responseBuilder.AppendLine(headerLine);

                if (headerLine == String.Empty)
                {
                    break;
                }

                var headerReader = new StringReader(headerLine);
                string key = headerReader.ReadUntil(c => c == ':');
                headerReader.Read();
                headerReader.SkipWhitespace();
                string value = headerReader.ReadToEnd();

                response.SetHeader(key, value);
            }

            var request = new OwinRequest(env);

            Stream responseBody = null;

            // 100-199, 204, 304 and HEAD requests never have a response body
            if ((response.StatusCode >= 100 && response.StatusCode <= 199) ||
                response.StatusCode == 204 ||
                response.StatusCode == 304 ||
                request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
            {
                responseBody = new ContentLengthStream(stream, 0);
            }
            else
            {
                var transferEncoding = response.GetHeader("Transfer-Encoding");

                if (transferEncoding != null &&
                    transferEncoding.Equals("chunked", StringComparison.OrdinalIgnoreCase))
                {
                    responseBody = new ChunkedStream(stream);
                }
                else
                {
                    int contentLength = Int32.Parse(response.GetHeader("Content-Length"));
                    responseBody = new ContentLengthStream(stream, contentLength);
                }
            }

            string connection = response.GetHeader("Connection");

            if (responseBody == null ||
                (protocol.Equals("HTTP/1.1", StringComparison.OrdinalIgnoreCase) && 
                "Close".Equals(connection, StringComparison.OrdinalIgnoreCase)))
            {
                var ms = new MemoryStream();

                if (responseBody == null)
                {
                    stream.CopyTo(ms);
                }
                else
                {
                    responseBody.CopyTo(ms);
                }

                ms.Seek(0, SeekOrigin.Begin);
                responseBody = ms;
                stream.Close();
            }

            response.Body = responseBody;
        }
    }
}
