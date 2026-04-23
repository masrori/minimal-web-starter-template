using Orchestrate.Extensions;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace Orchestrate.Binaries
{
    public sealed class BinaryDataWriter : IDisposable
    {
        private byte[] _buffer;
        private int _position;
        private int _length;
        private bool _disposed;

        private static readonly Encoding Utf8 = Encoding.UTF8;

        public BinaryDataWriter(int initialSize = 1024)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(initialSize);
            _position = 0;
            _length = 0;
        }

        public long Position => _position;
        public long Length => _length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Ensure(int size)
        {
            if ((_position + size) <= _buffer.Length) return;
            Grow(size);
        }

        private void Grow(int extra)
        {
            int needed = _position + extra;
            int newSize = _buffer.Length * 2;
            if (newSize < needed) newSize = needed;

            var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _length);
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = newBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int value)
        {
            Ensure(4);
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.AsSpan(_position), value);
            _position += 4;
            if (_position > _length) _length = _position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            Ensure(8);
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.AsSpan(_position), value);
            _position += 8;
            if (_position > _length) _length = _position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            Ensure(2);
            BinaryPrimitives.WriteInt16LittleEndian(_buffer.AsSpan(_position), value);
            _position += 2;
            if (_position > _length) _length = _position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32(uint value)
        {
            Ensure(4);
            BinaryPrimitives.WriteUInt32LittleEndian(_buffer.AsSpan(_position), value);
            _position += 4;
            if (_position > _length) _length = _position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSingle(float value)
        {
            Ensure(4);
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.AsSpan(_position), BitConverter.SingleToInt32Bits(value));
            _position += 4;
            if (_position > _length) _length = _position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDouble(double value)
        {
            Ensure(8);
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.AsSpan(_position), BitConverter.DoubleToInt64Bits(value));
            _position += 8;
            if (_position > _length) _length = _position;
        }

        public void WriteDecimal(decimal value)
        {
            var bits = decimal.GetBits(value);
            Ensure(16);
            var span = _buffer.AsSpan(_position);
            for (int i = 0; i < 4; i++) BinaryPrimitives.WriteInt32LittleEndian(span.Slice(i * 4), bits[i]);
            _position += 16;
            if (_position > _length) _length = _position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            Ensure(1);
            _buffer[_position++] = value;
            if (_position > _length) _length = _position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBoolean(bool value) => WriteByte(value ? (byte)1 : (byte)0);

        public void WriteString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteUInt16(0);
                return;
            }

            int byteCount = Utf8.GetByteCount(value);
            WriteUInt16((ushort)byteCount);
            Ensure(byteCount);
            Utf8.GetBytes(value.AsSpan(), _buffer.AsSpan(_position, byteCount));
            _position += byteCount;
            if (_position > _length) _length = _position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16(ushort value)
        {
            Ensure(2);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.AsSpan(_position), value);
            _position += 2;
            if (_position > _length) _length = _position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt64(ulong value)
        {
            Ensure(8);
            BinaryPrimitives.WriteUInt64LittleEndian(_buffer.AsSpan(_position), value);
            _position += 8;
            if (_position > _length) _length = _position;
        }

        // ===== RESERVE =====
        public int ReserveInt32()
        {
            int pos = _position;
            WriteInt32(0);
            return pos;
        }

        public void WriteInt32(int value, long pos)
        {
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.AsSpan((int)pos), value);
            if ((int)pos + 4 > _length) _length = (int)pos + 4;
        }

        // ===== BYTES =====
        public void WriteBytes(ReadOnlySpan<byte> data)
        {
            WriteInt32(data.Length);
            if (data.Length == 0) return;
            Ensure(data.Length);
            data.CopyTo(_buffer.AsSpan(_position));
            _position += data.Length;
            if (_position > _length) _length = _position;
        }

        // compatibility overload
        public void WriteBytes(byte[]? bytes)
        {
            if (bytes == null)
            {
                WriteInt32(0);
                return;
            }

            WriteBytes(bytes.AsSpan());
        }

        public void WriteDateTime(DateTime? value)
        {
            var ticks = value.HasValue ? value.Value.Ticks : DateTime.MinValue.Ticks;
            WriteInt64(ticks);
        }

        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            Ensure(16);
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.AsSpan(_position), value.Ticks);
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.AsSpan(_position + 8), value.Offset.Ticks);
            _position += 16;
            if (_position > _length) _length = _position;
        }

        public void WriteTimeSpan(TimeSpan value)
        {
            WriteInt64(value.Ticks);
        }

        public void WriteGuid(Guid? guid)
        {
            if (guid.HasValue)
            {
                WriteBoolean(true);
                var bytes = guid.Value.ToByteArray();
                Ensure(16);
                bytes.AsSpan().CopyTo(_buffer.AsSpan(_position));
                _position += 16;
                if (_position > _length) _length = _position;
            }
            else
            {
                WriteBoolean(false);
            }
        }

        // ===== ZERO COPY OUTPUT =====
        public ReadOnlyMemory<byte> GetMemory() => new ReadOnlyMemory<byte>(_buffer, 0, _length);

        public byte[] ToArray()
        {
            var arr = new byte[_length];
            Buffer.BlockCopy(_buffer, 0, arr, 0, _length);
            return arr;
        }

        public void Dispose()
        {
            if (_disposed) return;
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = Array.Empty<byte>();
            _disposed = true;
        }
    }
}