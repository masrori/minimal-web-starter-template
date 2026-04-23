using Orchestrate.Binaries;
using System.Data.Common;

namespace Orchestrate.Extensions
{
    public static class BinaryTypeMapper
    {
        public static int GetMethodIndex(this Type type)
        {
            if (type == typeof(string)) return 0;      // text, varchar, json, xml, uuid, name, citext
            if (type == typeof(bool)) return 1;        // boolean
            if (type == typeof(short)) return 2;       // smallint
            if (type == typeof(int)) return 3;         // integer
            if (type == typeof(long)) return 4;        // bigint
            if (type == typeof(float)) return 5;       // real
            if (type == typeof(double)) return 6;      // double precision
            if (type == typeof(decimal)) return 7;     // numeric, money
            if (type == typeof(byte[])) return 8;      // bytea
            if (type == typeof(DateTime)) return 9;    // date, timestamp, timestamptz (default)
            if (type == typeof(DateTimeOffset)) return 10; // timestamptz (if mapped)
            if (type == typeof(TimeSpan)) return 11;   // time, timetz, interval
            if (type == typeof(Guid)) return 12;       // uuid
            if (type == typeof(uint)) return 13;       // oid, xid, cid
            return -1;                                 // unknown
        }
        public static Type ConvertToType(this int index) => Types[index];
        public static void WriteCellData(this BinaryDataWriter writer, DbDataReader reader, int ordinal, int methodIndex)
        {
            CellWriters[methodIndex].Invoke(writer, reader, ordinal);
        }
        public static object? ReadCellData(this BinaryDataReader reader, int methodIndex)
        {
            return CellDataReaders[methodIndex].Invoke(reader);
        }
        private static readonly Action<BinaryDataWriter, DbDataReader, int>[] CellWriters = new Action<BinaryDataWriter, DbDataReader, int>[]
        {
            (w, r, o) => w.WriteString(r.IsDBNull(o) ? string.Empty : r.GetString(o)),
            (w, r, o) => w.WriteBoolean(r.IsDBNull(o) ? false : r.GetBoolean(o)),
            (w, r, o) => w.WriteInt16(r.IsDBNull(o) ? (short)0 : r.GetInt16(o)),
            (w, r, o) => w.WriteInt32(r.IsDBNull(o) ? 0 : r.GetInt32(o)),
            (w, r, o) => w.WriteInt64(r.IsDBNull(o) ? 0L : r.GetInt64(o)),
            (w, r, o) => w.WriteSingle(r.IsDBNull(o) ? 0f : r.GetFloat(o)),
            (w, r, o) => w.WriteDouble(r.IsDBNull(o) ? 0d : r.GetDouble(o)),
            (w, r, o) => w.WriteDecimal(r.IsDBNull(o) ? 0m : r.GetDecimal(o)),
            (w, r, o) => w.WriteBytes(r.IsDBNull(o) ? Array.Empty<byte>() : (byte[])r.GetValue(o)),
            (w, r, o) => w.WriteDateTime(r.IsDBNull(o) ? DateTime.MinValue : r.GetDateTime(o)),
            (w, r, o) => w.WriteDateTimeOffset(r.IsDBNull(o) ? DateTimeOffset.MinValue : r.GetFieldValue<DateTimeOffset>(o)),
            (w, r, o) => w.WriteTimeSpan(r.IsDBNull(o) ? TimeSpan.Zero : r.GetFieldValue<TimeSpan>(o)),
            (w, r, o) => w.WriteGuid(r.IsDBNull(o) ? Guid.Empty : r.GetGuid(o)),
            (w, r, o) => w.WriteUInt32(r.IsDBNull(o) ? 0u : r.GetFieldValue<uint>(o))
        };
        private static readonly Func<BinaryDataReader, object?>[] CellDataReaders = new Func<BinaryDataReader, object?>[]
        {
            r => r.ReadString(),
            r => r.ReadBoolean(),
            r => r.ReadInt16(),
            r => r.ReadInt32(),
            r => r.ReadInt64(),
            r => r.ReadSingle(),
            r => r.ReadDouble(),
            r => r.ReadDecimal(),
            r => r.ReadBytes(),
            r => r.ReadDateTime(),
            r => r.ReadDateTimeOffset(),
            r => r.ReadTimeSpan(),
            r => r.ReadGuid(),
            r => r.ReadUInt32()
        };
        private static readonly Type[] Types = new Type[]
        {
            typeof(string),         // 0
            typeof(bool),           // 1
            typeof(short),          // 2
            typeof(int),            // 3
            typeof(long),           // 4
            typeof(float),          // 5
            typeof(double),         // 6
            typeof(decimal),        // 7
            typeof(byte[]),         // 8
            typeof(DateTime),       // 9
            typeof(DateTimeOffset), // 10
            typeof(TimeSpan),       // 11
            typeof(Guid),           // 12
            typeof(uint)            // 13
        };
    }
}