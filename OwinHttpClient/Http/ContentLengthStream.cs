using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Owin.Client.Infrastructure;

namespace Owin.Client.Http
{
    internal class ContentLengthStream : DelegatingStream
    {
        private readonly Stream _stream;
        private readonly long _contentLength;
        private int _consumed;

        public ContentLengthStream(Stream stream, long contentLength)
            : base(stream)
        {
            _stream = stream;
            _contentLength = contentLength;
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_consumed >= _contentLength)
            {
                return 0;
            }

            var maxRead = (int)Math.Min(count, _contentLength - _consumed);

            int read = await _stream.ReadAsync(buffer, offset, maxRead, cancellationToken);

            _consumed += read;

            return read;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_consumed >= _contentLength)
            {
                return 0;
            }

            var maxRead = (int)Math.Min(count, _contentLength - _consumed);

            int read = _stream.Read(buffer, offset, maxRead);

            _consumed += read;

            return read;
        }
    }
}
