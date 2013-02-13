using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Owin.Middleware
{
    public class NetworkStreamFactory : IStreamFactory
    {
        public async Task<Stream> CreateStream(Uri uri)
        {
            Socket socket = null;

            IPAddress hostAddress;
            if (IPAddress.TryParse(uri.Host, out hostAddress))
            {
                socket = await Connect(hostAddress, uri.Port);
                if (socket.Connected)
                {
                    return await CreateStream(uri, socket);
                }
            }

            IPHostEntry hostEntry = Dns.GetHostEntry(uri.Host);

            foreach (IPAddress address in hostEntry.AddressList)
            {
                var ipe = new IPEndPoint(address, uri.Port);
                socket = await Connect(address, uri.Port);

                if (socket.Connected)
                {
                    return await CreateStream(uri, socket);
                }
            }

            return null;
        }

        private async Task<Stream> CreateStream(Uri uri, Socket socket)
        {
            Stream stream = new NetworkStream(socket);

            if (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                var sslStream = new SslStream(stream, leaveInnerStreamOpen: true);
                await sslStream.AuthenticateAsClientAsync(uri.Host);
                stream = sslStream;
            }

            return stream;
        }

        private static Task<Socket> Connect(IPAddress address, int port)
        {
            var ipe = new IPEndPoint(address, port);
            var socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var tcs = new TaskCompletionSource<Socket>();

            var sea = new SocketAsyncEventArgs();
            sea.RemoteEndPoint = ipe;
            sea.Completed += (sender, e) =>
            {
                if (e.SocketError != SocketError.Success)
                {
                    tcs.TrySetException(e.ConnectByNameError);
                }
                else
                {
                    tcs.TrySetResult(socket);
                }
            };

            bool completedSynchronously = socket.ConnectAsync(sea);

            return tcs.Task;
        }

    }
}
