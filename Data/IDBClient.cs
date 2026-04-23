using Microsoft.Data.SqlClient;
using Orchestrate.Binaries;
using Orchestrate.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Orchestrate.Data
{
    public interface IDBClient
    {
        DbParameter CreateParameter(string name, object? value);
        DbParameter CreateParameter(string name, object? value, DbType dbType);

        Task<bool> ExecuteNonQueryAsync(string commandText, params DbParameter[] parameters);
        Task ExecuteReaderAsync(Func<DbDataReader, Task> handler, string commandText, params DbParameter[] parameters);
        Task<T?> ExecuteScalarAsync<T>(string commandText, params DbParameter[] parameters);
        Task<DataTable> ExecuteDataTableAsync(string commandText, params DbParameter[] parameters);
        Task<bool> HasRecordsAsync(string commandText, params DbParameter[] parameters);
        Task<List<T>> FetchListAsync<T>(string commandText, Func<DbDataReader, T> callback, params DbParameter[] parameters);
        Task<Dictionary<TKey, TValue>> FetchDictionaryAsync<TKey, TValue>(string commandText, params DbParameter[] parameters) where TKey : notnull where TValue : notnull;
        Task<byte[]> ExecuteBinaryTableAsync(string commandText, params DbParameter[] parameters);
        Task<string> ExecuteHtmlTableAsync(string commandText, params DbParameter[] parameters);

        bool ExecuteNonQuery(string commandText, params DbParameter[] parameters);
        void ExecuteReader(Action<DbDataReader> handler, string commandText, params DbParameter[] parameters);
        T? ExecuteScalar<T>(string commandText, params DbParameter[] parameters);
        bool HasRecords(string commandText, params DbParameter[] parameters);
        List<T> FetchList<T>(string commandText, params DbParameter[] parameters);
        Dictionary<TKey, TValue> FetchDictionary<TKey, TValue>(string commandText, params DbParameter[] parameters) where TKey : notnull where TValue : notnull;
    }
    public abstract class DBClient : IDBClient
    {
        public bool ExecuteNonQuery(string commandText, params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(commandText, parameters))
            {
                try
                {
                    cmd.ExecuteNonQuery();
                    return true;
                }catch (Exception)
                {
                    return false;
                }
            }
        }
        public void ExecuteReader(Action<DbDataReader> handler, string commandText, params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(commandText, parameters))
            {
                try
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        handler(reader);
                    }
                }
                catch { }
            }
        }
        public T? ExecuteScalar<T>(string commandText, params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(commandText, parameters))
            {
                try
                {
                    object? res = cmd.ExecuteScalar();
                    if (res is null || res == DBNull.Value)
                        return default(T);

                    return (T)res;
                }
                catch
                {
                    return default(T);
                }
            }
        }
        public bool HasRecords(string commandText, params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(commandText, parameters))
            {
                try
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
                catch { return false; }
            }
        }
        public List<T> FetchList<T>(string commandText, params DbParameter[] parameters)
        {
            var list = new List<T>();
            ExecuteReader(reader =>
            {
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        var item = (T)Convert.ChangeType(reader.GetValue(0), typeof(T))!;
                        if (item is not null)
                        {
                            list.Add(item);
                        }
                    }
                }
            }, commandText, parameters);
            return list;
        }
        public Dictionary<TKey, TValue> FetchDictionary<TKey, TValue>(string commandText, params DbParameter[] parameters) where TKey : notnull where TValue : notnull
        {
            var dict = new Dictionary<TKey, TValue>();
            ExecuteReader(reader =>
            {
                while (reader.Read())
                {
                    var okey = !reader.IsDBNull(0) ? (TKey)Convert.ChangeType(reader.GetValue(0), typeof(TKey))! : default!;
                    var ovalue = !reader.IsDBNull(1) ? (TValue)Convert.ChangeType(reader.GetValue(1), typeof(TValue))! : default!;
                    if (okey is not null && ovalue is not null)
                    {
                        dict.Add(okey, ovalue);
                    }
                }
            }, commandText, parameters);
            return dict;
        }
        public async Task<byte[]> ExecuteBinaryTableAsync(string commandText, params DbParameter[] parameters)
        {
            using var writer = new Orchestrate.Binaries.BinaryDataWriter();
            using (var cmd = CreateCommand(commandText, parameters))
            {
                try
                {
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        var fieldCount = reader.FieldCount;
                        var types = new int[fieldCount];
                        writer.WriteByte((byte)fieldCount);
                        for (int i = 0; i < fieldCount; i++)
                        {
                            int funcIndex = reader.GetFieldType(i).GetMethodIndex();
                            types[i] = funcIndex;
                            writer.WriteByte((byte)funcIndex);
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return writer.ToArray();
        }

        public async Task<bool> ExecuteNonQueryAsync(string commandText, params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(commandText, parameters))
            {
                try
                {
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    return true;
                }
                catch (Exception ex)
                {
                    var logPath = Path.Combine(AppContext.BaseDirectory, "sql.log");
                    System.IO.File.AppendAllText(logPath, commandText + "\n\n" + ex.Message);
                    return false;
                }
            }
        }
        public async Task ExecuteReaderAsync(Func<DbDataReader, Task> handler, string commandText, params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(commandText, parameters))
            {
                try
                {
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        await handler(reader).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        public async Task<T?> ExecuteScalarAsync<T>(string commandText, params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(commandText, parameters))
            {
                try
                {
                    object? result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (result is null || result == DBNull.Value)
                    {
                        return default;
                    }
                    if (TryConvert(result, out T? converted)) return converted;
                    return (T?)Convert.ChangeType(result, typeof(T));
                }
                catch { return default(T); }
            }
        }
        public async Task<DataTable> ExecuteDataTableAsync(string commandText, params DbParameter[] parameters)
        {
            var table = new DataTable();
            using (var cmd = CreateCommand(commandText, parameters))
            {
                try
                {
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        table.Load(reader);
                    }
                }
                catch (Exception ex)
                {
                    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "sql.txt"), ex.ToString());
                    Console.WriteLine(ex.ToString());
                }
            }
            return table;
        }
        public async Task<bool> HasRecordsAsync(string commandText, params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(commandText, parameters))
            {
                try
                {
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        return reader.HasRows;
                    }
                }
                catch { return false; }
            }
        }
        public async Task<string> ExecuteHtmlTableAsync(string commandText, params DbParameter[] parameters)
        {
            var sb = new StringBuilder();
            using (var cmd = CreateCommand(commandText, parameters))
            {
                try
                {
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {

                        }
                    }
                }
                catch (Exception)
                {

                }
            }
            return sb.ToString();
        }
        public async Task<List<T>> FetchListAsync<T>(string commandText, Func<DbDataReader, T> callback, params DbParameter[] parameters)
        {
            var list = new List<T>();
            await ExecuteReaderAsync(async reader =>
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    list.Add(callback(reader));
                }
            }, commandText, parameters).ConfigureAwait(false);
            return list;
        }
        public async Task<Dictionary<TKey, TValue>> FetchDictionaryAsync<TKey, TValue>(string commandText, params DbParameter[] parameters) where TKey : notnull where TValue : notnull
        {
            var dict = new Dictionary<TKey, TValue>();
            await ExecuteReaderAsync(async reader =>
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var rawk = reader.GetValue(0);
                    var rawv = reader.GetValue(1);
                    if (TryConvert(rawk, out TKey? okey) && TryConvert(rawv, out TValue? ovalue) && okey is not null && ovalue is not null)
                    {
                        dict.Add(okey, ovalue);
                    }
                }
            }, commandText, parameters).ConfigureAwait(false);
            return dict;
        }

        //overridable
        protected abstract DbCommand CreateCommand(string commandText, params DbParameter[] parameters);

        public abstract DbParameter CreateParameter(string name, object? value);
        public abstract DbParameter CreateParameter(string name, object? value, DbType dbType);
        public abstract ConnectionState Open();
        public abstract Task<ConnectionState> OpenAsync();
        private static bool TryConvert<T>(object? input, out T? result)
        {
            result = default;
            if (input is null || input == DBNull.Value) return false;
            if (input is T t) { result = t; return true; }

            var target = typeof(T);
            try
            {
                switch (Type.GetTypeCode(target))
                {
                    case TypeCode.Boolean: result = (T)(object)Convert.ToBoolean(input); return true;
                    case TypeCode.Byte: result = (T)(object)Convert.ToByte(input); return true;
                    case TypeCode.SByte: result = (T)(object)Convert.ToSByte(input); return true;
                    case TypeCode.Int16: result = (T)(object)Convert.ToInt16(input); return true;
                    case TypeCode.UInt16: result = (T)(object)Convert.ToUInt16(input); return true;
                    case TypeCode.Int32: result = (T)(object)Convert.ToInt32(input); return true;
                    case TypeCode.UInt32: result = (T)(object)Convert.ToUInt32(input); return true;
                    case TypeCode.Int64: result = (T)(object)Convert.ToInt64(input); return true;
                    case TypeCode.UInt64: result = (T)(object)Convert.ToUInt64(input); return true;
                    case TypeCode.Single: result = (T)(object)Convert.ToSingle(input); return true;
                    case TypeCode.Double: result = (T)(object)Convert.ToDouble(input); return true;
                    case TypeCode.Decimal: result = (T)(object)Convert.ToDecimal(input); return true;
                    case TypeCode.String: result = (T)(object)Convert.ToString(input)!; return true;
                    case TypeCode.DateTime: result = (T)(object)Convert.ToDateTime(input); return true;
                    default:
                        if (target == typeof(Guid))
                        {
                            if (input is Guid g) { result = (T)(object)g; return true; }
                            if (input is string gs && Guid.TryParse(gs, out var pg)) { result = (T)(object)pg; return true; }
                        }
                        result = (T?)Convert.ChangeType(input, target);
                        return result is not null;
                }
            }
            catch { return false; }
        }
    }

    public class PageRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string Search { get; set; } = "";
        public int SortedColumnIndex { get; set; } = 0;
        public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
    }

    public class PaginationBuilder
    {
        private readonly PageRequest _request;

        public PaginationBuilder(PageRequest request)
        {
            _request = request;
        }

        public Dictionary<int, string> SortMap { get; set; } = new();
        public Column[] Columns { get; set; } = Array.Empty<Column>();
        public string From { get; set; } = "";
        public Func<string, string>? FilterMethod = null;

        public async Task<byte[]> ExecuteAsync(IDBClient db)
        {
            using var writer = new BinaryDataWriter();

            try
            {
                string sSQLFrom = this.From;
                if (!string.IsNullOrEmpty(_request.Search) && FilterMethod != null)
                {
                    var where = FilterMethod(_request.Search);
                    if (!string.IsNullOrWhiteSpace(where))
                        sSQLFrom += " WHERE " + where;
                }

                int totalRecords = await db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM " + sSQLFrom
                );

                var sb = new StringBuilder(4096);
                sb.Append("SELECT ");

                for (int i = 0; i < Columns.Length; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(Columns[i].Name);
                }

                sb.Append(" FROM ").Append(sSQLFrom);
                var orderBy = SortMap.TryGetValue(_request.SortedColumnIndex, out var s)
                    ? s
                    : Columns[0].Name;

                sb.Append(" ORDER BY ").Append(orderBy);
                sb.Append(_request.SortOrder == SortOrder.Descending ? " DESC " : " ASC ");
                int offset = _request.Page * _request.PageSize;

                sb.Append(" OFFSET ").Append(offset).Append(" ROWS ");
                sb.Append(" FETCH NEXT ").Append(_request.PageSize).Append(" ROWS ONLY");

                writer.WriteBoolean(true);   // res.ok
                writer.WriteBoolean(true);   // validation

                int totalPages = (int)Math.Ceiling((double)totalRecords / _request.PageSize);

                writer.WriteInt32(totalRecords);
                writer.WriteInt32(_request.Page + 1); // JS pakai 1-based
                writer.WriteInt32(_request.PageSize);
                writer.WriteInt32(totalPages);

                await db.ExecuteReaderAsync(async r =>
                {
                    int fieldCount = r.FieldCount;
                    var types = new int[fieldCount];
                    writer.WriteByte((byte)fieldCount);

                    for (int i = 0; i < fieldCount; i++)
                    {
                        int typeIndex = r.GetFieldType(i).GetMethodIndex();

                        if (typeIndex < 0)
                            throw new NotSupportedException($"Unsupported type: {r.GetFieldType(i)}");

                        types[i] = typeIndex;
                        writer.WriteByte((byte)typeIndex);
                    }
                    var rows = new List<object[]>();
                    int countPos = writer.ReserveInt32();
                    int rowCount = 0;
                    while (await r.ReadAsync().ConfigureAwait(false))
                    {
                        for(int i=0; i < fieldCount; i++)
                        {
                            BinaryTypeMapper.WriteCellData(writer, r, i, types[i]);
                        }
                        rowCount++;
                    }
                    writer.WriteInt32(rowCount, countPos);

                }, sb.ToString());
            }
            catch (Exception ex)
            {
                writer.WriteBoolean(false);
                writer.WriteString(ex.Message);
            }
            return writer.ToArray();
        }
    }
}