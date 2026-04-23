namespace Orchestrate.Data
{
    public enum DbDataType : byte
    {
        String = 0,
        Int32 = 1,
        Int64 = 3,
        Int16 = 5
    }
    public class Column
    {
        public Column(string name, DbDataType dataType)
        {
            Name = name;
            DataType = dataType;
        }

        public string Name { get; set; } = "";
        public DbDataType DataType { get; set; } = DbDataType.String;
    }
    public class Table
    {
        private Column[] _columns;
        private string _sql_from = "";
        private string _sql_where = "";
        private Table(Column[] columns)
        {
            _columns = columns;
        }
        public static Table Select(Column[] columns)
        {
            return new Table(columns);
        }

        public Table From(string sql)
        {
            _sql_from = sql;
            return this;
        }
        public Table Where(Func<string, string> callback)
        {
            _sql_from += callback("hello");
            return this;
        }
        public async Task<byte[]> ExecuteAsync(IDBClient db)
        {
            string sql = "SELECT " + string.Join<Column>(", ", _columns) + " " + _sql_from;
            return System.Text.Encoding.UTF8.GetBytes(sql);
        }
    }
}
