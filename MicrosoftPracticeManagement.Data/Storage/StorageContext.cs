using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace MicrosoftPracticeManagement.Data.Storage
{
    public class StorageContext
    {
        private readonly StorageConfiguration _config;
        private readonly ILogger<StorageContext> _logger;
        private TableServiceClient? _tableServiceClient;
        private BlobServiceClient? _blobServiceClient;
        private bool _isAzureTableAvailable = false;
        private bool _isAzureBlobAvailable = false;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ITableEntity>> _inMemoryTables = new();

        public StorageContext(StorageConfiguration config, ILogger<StorageContext> logger)
        {
            _config = config;
            _logger = logger;
            InitializeClients();
        }

        public bool IsAzureTableAvailable => _isAzureTableAvailable;
        public bool IsAzureBlobAvailable => _isAzureBlobAvailable;

        private void InitializeClients()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_config.TableStorageConnection))
                {
                    var options = new TableClientOptions();
                    options.Retry.MaxRetries = 3;
                    options.Retry.NetworkTimeout = TimeSpan.FromSeconds(10);

                    _tableServiceClient = new TableServiceClient(_config.TableStorageConnection, options);
                    // Test connectivity
                    _tableServiceClient.Query().Take(1).ToList();
                    _isAzureTableAvailable = true;
                    _logger.LogInformation("Azure Table Storage connection successfully established.");
                }
            }
            catch (Exception ex)
            {
                _isAzureTableAvailable = false;
                _logger.LogWarning("Azure Table Storage connection could not be established ({Message}). Falling back to local storage engine.", ex.Message);
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(_config.BlobStorageConnection))
                {
                    var options = new BlobClientOptions();
                    options.Retry.MaxRetries = 3;
                    options.Retry.NetworkTimeout = TimeSpan.FromSeconds(10);

                    _blobServiceClient = new BlobServiceClient(_config.BlobStorageConnection, options);
                    _blobServiceClient.GetBlobContainers().Take(1).ToList();
                    _isAzureBlobAvailable = true;
                    _logger.LogInformation("Azure Blob Storage connection successfully established.");
                }
            }
            catch (Exception ex)
            {
                _isAzureBlobAvailable = false;
                _logger.LogWarning("Azure Blob Storage connection could not be established ({Message}). Local mock blob engine enabled.", ex.Message);
            }
        }

        public TableClient GetTableClient(string tableName)
        {
            if (_isAzureTableAvailable && _tableServiceClient != null)
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                try
                {
                    tableClient.CreateIfNotExists();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not create table {TableName} on Azure Storage: {Message}", tableName, ex.Message);
                }
                return tableClient;
            }

            // Fallback TableClient wrapper or in-memory dictionary
            return _tableServiceClient?.GetTableClient(tableName) ?? new TableClient("UseDevelopmentStorage=true", tableName);
        }

        public BlobContainerClient GetBlobContainerClient(string containerName)
        {
            var prefixedName = string.IsNullOrEmpty(_config.BlobContainerPrefix) 
                ? containerName 
                : $"{_config.BlobContainerPrefix}-{containerName}".ToLowerInvariant();

            if (_isAzureBlobAvailable && _blobServiceClient != null)
            {
                var container = _blobServiceClient.GetBlobContainerClient(prefixedName);
                try
                {
                    container.CreateIfNotExists();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not create container {ContainerName} on Azure Blob Storage: {Message}", prefixedName, ex.Message);
                }
                return container;
            }

            return _blobServiceClient?.GetBlobContainerClient(prefixedName) ?? new BlobContainerClient("UseDevelopmentStorage=true", prefixedName);
        }

        public ConcurrentDictionary<string, ITableEntity> GetInMemoryTable(string tableName)
        {
            return _inMemoryTables.GetOrAdd(tableName, _ => new ConcurrentDictionary<string, ITableEntity>());
        }
    }
}
