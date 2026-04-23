using System.Buffers;
using System.Buffers.Binary;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Orchestrate.Extensions;

namespace Orchestrate.Binaries
{
    public sealed class BinaryDataReader : IDisposable
    {
        private byte[] _buffer;
        private int _start;
        private int _pos;
        private int _length;
        private bool _ownsBuffer;

        private static readonly Encoding Utf8 = Encoding.UTF8;

        public BinaryDataReader(ReadOnlyMemory<byte> memory)
        {
            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> seg))
            {
                _buffer = seg.Array!;
                _start = seg.Offset;
                _pos = 0;
                _length = seg.Count;
                _ownsBuffer = false;
            }
            else
            {
                _buffer = ArrayPool<byte>.Shared.Rent(memory.Length);
                memory.Span.CopyTo(_buffer.AsSpan(0, memory.Length));
                _start = 0;
                _pos = 0;
                _length = memory.Length;
                _ownsBuffer = true;
            }
        }

        public BinaryDataReader(Stream stream)
        {
            if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> seg))
            {
                _buffer = seg.Array!;
                _start = seg.Offset;
                _pos = (int)ms.Position;
                _length = (int)ms.Length - _start;
                _ownsBuffer = false;
            }
            else
            {
                using var tmp = new MemoryStream();
                stream.CopyTo(tmp);
                var arr = tmp.ToArray();
                _buffer = arr;
                _start = 0;
                _pos = 0;
                _length = arr.Length;
                _ownsBuffer = false;
            }
        }

        public long Position => _pos;
        public long Length => _length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<byte> SpanFrom(int count) => _buffer.AsSpan(_start + _pos, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBoolean() => ReadByte() != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte() => _buffer[_start + _pos++];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte() => (sbyte)_buffer[_start + _pos++];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            short v = BinaryPrimitives.ReadInt16LittleEndian(SpanFrom(2));
            _pos += 2;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            ushort v = BinaryPrimitives.ReadUInt16LittleEndian(SpanFrom(2));
            _pos += 2;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
        {
            int v = BinaryPrimitives.ReadInt32LittleEndian(SpanFrom(4));
            _pos += 4;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            uint v = BinaryPrimitives.ReadUInt32LittleEndian(SpanFrom(4));
            _pos += 4;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            long v = BinaryPrimitives.ReadInt64LittleEndian(SpanFrom(8));
            _pos += 8;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            ulong v = BinaryPrimitives.ReadUInt64LittleEndian(SpanFrom(8));
            _pos += 8;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadSingle()
        {
            int iv = ReadInt32();
            return BitConverter.Int32BitsToSingle(iv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            long lv = ReadInt64();
            return BitConverter.Int64BitsToDouble(lv);
        }

        public decimal ReadDecimal()
        {
            int[] bits = new int[4];
            for (int i = 0; i < 4; i++) bits[i] = ReadInt32();
            return new decimal(bits);
        }

        public string ReadString()
        {
            var len = ReadUInt16();
            if (len == 0) return string.Empty;
            var s = Utf8.GetString(_buffer, _start + _pos, len);
            _pos += len;
            return s;
        }

        public DateTime ReadDateTime()
        {
            var ticks = ReadInt64();
            return ticks == 0 ? DateTime.MinValue : new DateTime(ticks, DateTimeKind.Utc);
        }

        public DateTimeOffset ReadDateTimeOffset()
        {
            var ticks = ReadInt64();
            var offset = ReadInt64();
            return new DateTimeOffset(ticks, new TimeSpan(offset));
        }

        public TimeSpan ReadTimeSpan() => new TimeSpan(ReadInt64());

        public object ReadDateTimeOrDBNull()
        {
            long ticks = ReadInt64();
            long defaultDate = (new DateTime(1900, 1, 1)).Ticks;
            return (ticks == 0 || ticks == defaultDate) ? DBNull.Value : new DateTime(ticks, DateTimeKind.Utc);
        }

        public DateTime? ReadNullableDateTime()
        {
            long ticks = ReadInt64();
            return ticks == 0 ? (DateTime?)null : new DateTime(ticks);
        }

        public bool Read() => _pos < _length;

        public byte[] ReadBytes()
        {
            var length = ReadInt32();
            if (length == 0) return Array.Empty<byte>();
            var res = new byte[length];
            Buffer.BlockCopy(_buffer, _start + _pos, res, 0, length);
            _pos += length;
            return res;
        }

        public DataTable ReadDataTable()
        {
            var table = new DataTable();
            var colCount = ReadByte();
            if (colCount == 0) return table;

            var types = new int[colCount];
            for (int i = 0; i < colCount; i++)
            {
                var colName = ReadString();
                types[i] = ReadByte();
                table.Columns.Add(colName, types[i].ConvertToType());
            }

            try
            {
                var rowCount = ReadInt32();
                for (int i = 0; i < rowCount; i++)
                {
                    var values = new object?[colCount];
                    for (int j = 0; j < colCount; j++)
                    {
                        values[j] = this.ReadCellData(types[j]);
                    }
                    table.Rows.Add(values);
                }
            }
            catch { }

            return table;
        }

        public Guid ReadGuid()
        {
            if (ReadBoolean())
            {
                var res = new byte[16];
                Buffer.BlockCopy(_buffer, _start + _pos, res, 0, 16);
                _pos += 16;
                return new Guid(res);
            }
            return Guid.Empty;
        }

        public void Dispose()
        {
            if (_ownsBuffer && _buffer != null && _buffer.Length > 0)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
            }
            _buffer = Array.Empty<byte>();
            _start = 0;
            _pos = 0;
            _length = 0;
            _ownsBuffer = false;
            GC.SuppressFinalize(this);
        }
    }
}