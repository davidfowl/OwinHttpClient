using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Owin
{
    public class NetworkStreamFactory : IStreamFactory
    {
        public async Task<Stream> CreateStream(string host, int port)
        {
            Socket socket = null;

            IPAddress hostAddress;
            if (IPAddress.TryParse(host, out hostAddress))
            {
                socket = await Connect(hostAddress, port);
                if (socket.Connected)
                {
                    return new NetworkStream(socket);
                }
            }

            IPHostEntry hostEntry = Dns.GetHostEntry(host);

            foreach (IPAddress address in hostEntry.AddressList)
            {
                var ipe = new IPEndPoint(address, port);
                socket = await Connect(address, port);

                if (socket.Connected)
                {
                    return new NetworkStream(socket);
                }
            }

            return null;
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
