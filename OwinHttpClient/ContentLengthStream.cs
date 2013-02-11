using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Owin
{
    internal class ContentLengthStream : Stream
    {
        private readonly Stream _stream;
        private readonly int _contentLength;
        private int _consumed;

        public ContentLengthStream(Stream stream, int contentLength)
        {
            _stream = stream;
            _contentLength = contentLength;
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return _contentLength; }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_consumed >= _contentLength)
            {
                return 0;
            }

            int maxRead = Math.Min(count, _contentLength - _consumed);

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

            int maxRead = Math.Min(count, _contentLength - _consumed);

            int read = _stream.Read(buffer, offset, maxRead);

            _consumed += read;

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
