using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Orchestrate.Data
{
    public class SqlDbClient : DBClient, IAsyncDisposable, IDisposable
    {
        private bool disposedValue;
        private readonly SqlConnection _connection;
        private SqlDbClient(string connstring) => _connection = new SqlConnection(connstring);
        public override DbParameter CreateParameter(string name, object? value) => new SqlParameter(name, value != null ? value : DBNull.Value);
        public override DbParameter CreateParameter(string name, object? value, DbType dbType) => new SqlParameter(name, dbType) { Value = value != null ? value : DBNull.Value };
        protected override DbCommand CreateCommand(string commandText, params DbParameter[] parameters)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add(parameters);
            return cmd;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _connection.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }
        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    await _connection.CloseAsync();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SqlDbClient()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await DisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static SqlDbClient Create(string connstring) => new SqlDbClient(connstring);

        public override ConnectionState Open()
        {
            this._connection.Open();
            return this._connection.State;
        }

        public override async Task<ConnectionState> OpenAsync()
        {
            await this._connection.OpenAsync().ConfigureAwait(false);
            return this._connection.State;
        }
    }
}