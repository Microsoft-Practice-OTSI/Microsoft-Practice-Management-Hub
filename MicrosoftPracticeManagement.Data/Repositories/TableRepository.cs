using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using MicrosoftPracticeManagement.Data.Storage;
using System.Collections.Concurrent;
using System.Reflection;

namespace MicrosoftPracticeManagement.Data.Repositories
{
    public class TableRepository<T> : ITableRepository<T> where T : class, ITableEntity, new()
    {
        protected readonly StorageContext _storageContext;
        protected readonly ILogger _logger;
        public string TableName { get; }
        private readonly TableClient _tableClient;
        private readonly ConcurrentDictionary<string, ITableEntity> _inMemoryStore;

        public TableRepository(string tableName, StorageContext storageContext, ILogger logger)
        {
            TableName = tableName;
            _storageContext = storageContext;
            _logger = logger;
            _tableClient = _storageContext.GetTableClient(tableName);
            _inMemoryStore = _storageContext.GetInMemoryTable(tableName);
        }

        private string MakeKey(string partitionKey, string rowKey) => $"{partitionKey}____{rowKey}";

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            if (_storageContext.IsAzureTableAvailable)
            {
                try
                {
                    var results = new List<T>();
                    await foreach (var page in _tableClient.QueryAsync<T>().AsPages())
                    {
                        results.AddRange(page.Values);
                    }
                    return results;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to query table {TableName} from Azure Storage: {Message}. Serving from in-memory cache.", TableName, ex.Message);
                }
            }

            return _inMemoryStore.Values.OfType<T>().ToList();
        }

        public virtual async Task<IEnumerable<T>> QueryAsync(string filter)
        {
            if (_storageContext.IsAzureTableAvailable)
            {
                try
                {
                    var results = new List<T>();
                    await foreach (var item in _tableClient.QueryAsync<T>(filter))
                    {
                        results.Add(item);
                    }
                    return results;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to execute filter query on {TableName}: {Message}", TableName, ex.Message);
                }
            }

            // Fallback in-memory query
            return _inMemoryStore.Values.OfType<T>().ToList();
        }

        public virtual async Task<T?> GetByIdAsync(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return null;

            if (_storageContext.IsAzureTableAvailable)
            {
                try
                {
                    NullableResponse<T> response = await _tableClient.GetEntityIfExistsAsync<T>(partitionKey, rowKey);
                    if (response.HasValue)
                        return response.Value;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to get entity {PartitionKey}/{RowKey} from {TableName}: {Message}", partitionKey, rowKey, TableName, ex.Message);
                }
            }

            var key = MakeKey(partitionKey, rowKey);
            if (_inMemoryStore.TryGetValue(key, out var entity) && entity is T typedEntity)
            {
                return typedEntity;
            }

            return null;
        }

        public virtual async Task AddAsync(T entity)
        {
            EnsureUtcDateTimes(entity);
            var key = MakeKey(entity.PartitionKey, entity.RowKey);
            _inMemoryStore[key] = entity;

            if (_storageContext.IsAzureTableAvailable)
            {
                try
                {
                    await _tableClient.AddEntityAsync(entity);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to add entity to Azure Table {TableName}: {Message}", TableName, ex.Message);
                }
            }
        }

        public virtual async Task UpdateAsync(T entity)
        {
            EnsureUtcDateTimes(entity);
            var key = MakeKey(entity.PartitionKey, entity.RowKey);
            _inMemoryStore[key] = entity;

            if (_storageContext.IsAzureTableAvailable)
            {
                try
                {
                    await _tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to update entity in Azure Table {TableName}: {Message}", TableName, ex.Message);
                }
            }
        }

        public virtual async Task UpsertAsync(T entity)
        {
            EnsureUtcDateTimes(entity);
            var key = MakeKey(entity.PartitionKey, entity.RowKey);
            _inMemoryStore[key] = entity;

            if (_storageContext.IsAzureTableAvailable)
            {
                try
                {
                    await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to upsert entity to Azure Table {TableName}: {Message}", TableName, ex.Message);
                }
            }
        }

        public virtual async Task DeleteAsync(string partitionKey, string rowKey)
        {
            var key = MakeKey(partitionKey, rowKey);
            _inMemoryStore.TryRemove(key, out _);

            if (_storageContext.IsAzureTableAvailable)
            {
                try
                {
                    await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to delete entity from Azure Table {TableName}: {Message}", TableName, ex.Message);
                }
            }
        }

        public virtual async Task BatchUpsertAsync(IEnumerable<T> entities)
        {
            var entityList = entities.ToList();
            foreach (var entity in entityList)
            {
                EnsureUtcDateTimes(entity);
                var key = MakeKey(entity.PartitionKey, entity.RowKey);
                _inMemoryStore[key] = entity;
            }

            if (_storageContext.IsAzureTableAvailable && entityList.Count > 0)
            {
                try
                {
                    var grouped = entityList.GroupBy(e => e.PartitionKey);
                    foreach (var group in grouped)
                    {
                        var chunks = group.Chunk(100);
                        foreach (var chunk in chunks)
                        {
                            var batch = new List<TableTransactionAction>();
                            foreach (var item in chunk)
                            {
                                batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, item));
                            }
                            await _tableClient.SubmitTransactionAsync(batch);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to batch upsert into Azure Table {TableName}: {Message}", TableName, ex.Message);
                }
            }
        }

        private void EnsureUtcDateTimes(T entity)
        {
            var properties = entity.GetType().GetProperties();
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(DateTime))
                {
                    var value = (DateTime)prop.GetValue(entity)!;
                    if (value.Kind == DateTimeKind.Unspecified)
                    {
                        prop.SetValue(entity, DateTime.SpecifyKind(value, DateTimeKind.Utc));
                    }
                    else if (value.Kind == DateTimeKind.Local)
                    {
                        prop.SetValue(entity, value.ToUniversalTime());
                    }
                }
                else if (prop.PropertyType == typeof(DateTime?))
                {
                    var value = (DateTime?)prop.GetValue(entity);
                    if (value.HasValue)
                    {
                        if (value.Value.Kind == DateTimeKind.Unspecified)
                        {
                            prop.SetValue(entity, DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
                        }
                        else if (value.Value.Kind == DateTimeKind.Local)
                        {
                            prop.SetValue(entity, value.Value.ToUniversalTime());
                        }
                    }
                }
            }
        }
    }
}
