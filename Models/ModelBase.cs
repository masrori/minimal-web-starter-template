using Orchestrate.Data;

namespace Orchestrate.Models
{
    public abstract class ModelBase
    {
        public abstract Task<List<ModelBase>> GetModelsAsync();
        public abstract Task<T> CreateAsync<T>(IDBClient db);
        public abstract Task<bool> UpdateAsync(IDBClient db);
        public abstract Task<bool> DeleteAsync(IDBClient db);
    }
}
