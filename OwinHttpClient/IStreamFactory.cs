using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Owin
{
    public interface IStreamFactory
    {
        Task<Stream> CreateStream(string host, int port);
    }
}
