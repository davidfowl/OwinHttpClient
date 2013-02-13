using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Owin.Infrastructure;

namespace Owin.Http
{
    internal class ChunkedStream : Stream
    {
        private readonly Stream _stream;
        private int _consumed;
        private int? _chunkLength;

        public ChunkedStream(Stream networkStream)
        {
            _stream = networkStream;
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
            get { throw new NotImplementedException(); }
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
            if (_chunkLength == null)
            {
                string rawLength = _stream.ReadLine();

                int length = Int32.Parse(rawLength, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                if (length == 0)
                {
                    return await Task.FromResult<int>(0);
                }

                _chunkLength = length;
            }

            int maxRead = Math.Min(count - offset, _chunkLength.Value - _consumed);

            int read = await _stream.ReadAsync(buffer, offset, maxRead, cancellationToken);

            _consumed += read;

            if (_consumed >= _chunkLength)
            {
                _stream.ReadLine();

                _chunkLength = null;

                _consumed = 0;
            }

            return read;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_chunkLength == null)
            {
                string rawLength = _stream.ReadLine();

                int length = Int32.Parse(rawLength, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                if (length == 0)
                {
                    return 0;
                }

                _chunkLength = length;
            }

            int maxRead = Math.Min(count - offset, _chunkLength.Value - _consumed);

            int read = _stream.Read(buffer, offset, maxRead);

            _consumed += read;

            if (_consumed >= _chunkLength)
            {
                _stream.ReadLine();

                _chunkLength = null;

                _consumed = 0;
            }

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
