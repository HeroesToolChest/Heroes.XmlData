using System.Text;

namespace Heroes.StormData.Text;

internal sealed class KeyValueTextReader : IDisposable
{
    private readonly Stream _stream;

    private byte[] _buffer;
    private int _start;
    private int _end;
    private bool _isEndOfStream;
    private bool _isFirstRead = true;

    public KeyValueTextReader(Stream stream, int bufferSize = 64 * 1024)
    {
        _stream = stream;
        _buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
    }

    public bool TryRead(out ReadOnlySpan<byte> key, out ReadOnlySpan<byte> value)
    {
        while (TryReadLine(out ReadOnlySpan<byte> line))
        {
            int eq = line.IndexOf((byte)'=');
            key = eq < 0 ? line : line[..eq];
            value = eq < 0 ? [] : line[(eq + 1)..];

            if (!key[Ascii.Trim(key)].IsEmpty)
                return true;
        }

        key = value = default;
        return false;
    }

    public void Dispose() => ArrayPool<byte>.Shared.Return(_buffer);

    private bool TryReadLine(out ReadOnlySpan<byte> line)
    {
        while (true)
        {
            ReadOnlySpan<byte> pending = _buffer.AsSpan(_start, _end - _start);

            int newLine = pending.IndexOf((byte)'\n');

            if (newLine >= 0)
            {
                line = pending[..newLine];
                _start += newLine + 1;

                break;
            }

            if (_isEndOfStream)
            {
                if (pending.IsEmpty)
                {
                    line = default;

                    return false;
                }

                line = pending; // last line without a trailing '\n'
                _start = _end;

                break;
            }

            Fill();
        }

        if (!line.IsEmpty && line[^1] == (byte)'\r')
            line = line[..^1];

        return true;
    }

    private void Fill()
    {
        int pendingLength = _end - _start;

        if (pendingLength == _buffer.Length) // a single line is larger than the buffer
        {
            byte[] biggerBuffer = ArrayPool<byte>.Shared.Rent(_buffer.Length * 2);

            _buffer.AsSpan(0, pendingLength).CopyTo(biggerBuffer);

            ArrayPool<byte>.Shared.Return(_buffer);

            _buffer = biggerBuffer;
        }
        else if (_start > 0)
        {
            _buffer.AsSpan(_start, pendingLength).CopyTo(_buffer); // move the partial line to the front
        }

        _start = 0;
        _end = pendingLength;

        int read = _stream.Read(_buffer, _end, _buffer.Length - _end);
        if (read == 0)
        {
            _isEndOfStream = true;

            return;
        }

        _end += read;

        if (_isFirstRead)
        {
            _isFirstRead = false;

            // the stream may return fewer bytes than the BOM, so keep reading until there are enough to check
            while (_end < Encoding.UTF8.Preamble.Length && (read = _stream.Read(_buffer, _end, _buffer.Length - _end)) > 0)
                _end += read;

            if (_buffer.AsSpan(0, _end).StartsWith(Encoding.UTF8.Preamble))
                _start = Encoding.UTF8.Preamble.Length;
        }
    }
}
