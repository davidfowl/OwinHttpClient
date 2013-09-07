using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Owin.Client.Infrastructure;

namespace Owin.Client.Http
{
    internal class ChunkedStream : DelegatingStream
    {
        private readonly Stream _stream;
        private int _consumed;
        private int? _chunkLength;

        public ChunkedStream(Stream stream)
            : base(stream)
        {
            _stream = stream;
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
    }
}
