using Azure.Data.Tables;

namespace MicrosoftPracticeManagement.Data.Repositories
{
    public interface ITableRepository<T> where T : class, ITableEntity, new()
    {
        string TableName { get; }
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> QueryAsync(string filter);
        Task<T?> GetByIdAsync(string partitionKey, string rowKey);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task UpsertAsync(T entity);
        Task DeleteAsync(string partitionKey, string rowKey);
        Task BatchUpsertAsync(IEnumerable<T> entities);
    }
}
