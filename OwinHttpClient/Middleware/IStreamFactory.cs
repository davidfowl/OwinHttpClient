using System;
using System.IO;
using System.Threading.Tasks;

namespace Owin.Middleware
{
    public interface IStreamFactory
    {
        Task<Stream> CreateStream(Uri uri);
    }
}
