using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using MicrosoftPracticeManagement.Data.Storage;
using System.Collections.Concurrent;

namespace MicrosoftPracticeManagement.Data.BlobStorage
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly StorageContext _storageContext;
        private readonly ILogger<AzureBlobStorageService> _logger;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>> _mockStorage = new();

        public AzureBlobStorageService(StorageContext storageContext, ILogger<AzureBlobStorageService> logger)
        {
            _storageContext = storageContext;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(string containerName, string fileName, Stream content, string contentType)
        {
            if (_storageContext.IsAzureBlobAvailable)
            {
                try
                {
                    var container = _storageContext.GetBlobContainerClient(containerName);
                    var blobClient = container.GetBlobClient(fileName);

                    content.Position = 0;
                    var options = new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                    };

                    await blobClient.UploadAsync(content, options);
                    return blobClient.Uri.ToString();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to upload blob {FileName} to container {ContainerName}: {Message}", fileName, containerName, ex.Message);
                }
            }

            // Mock in-memory storage fallback
            using var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            var containerDict = _mockStorage.GetOrAdd(containerName, _ => new ConcurrentDictionary<string, byte[]>());
            containerDict[fileName] = ms.ToArray();

            return $"/api/blob/{containerName}/{fileName}";
        }

        public async Task<Stream?> DownloadFileAsync(string containerName, string fileName)
        {
            if (_storageContext.IsAzureBlobAvailable)
            {
                try
                {
                    var container = _storageContext.GetBlobContainerClient(containerName);
                    var blobClient = container.GetBlobClient(fileName);
                    if (await blobClient.ExistsAsync())
                    {
                        var download = await blobClient.DownloadStreamingAsync();
                        return download.Value.Content;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to download blob {FileName} from container {ContainerName}: {Message}", fileName, containerName, ex.Message);
                }
            }

            if (_mockStorage.TryGetValue(containerName, out var containerDict) && containerDict.TryGetValue(fileName, out var data))
            {
                return new MemoryStream(data);
            }

            // Default fallback dummy stream for sample files
            var dummyText = $"Microsoft Practice Hub Sample Content for {containerName}/{fileName}\nGenerated: {DateTime.UtcNow:u}\nConfidential - Microsoft Practice Delivery";
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(dummyText));
        }

        public async Task<bool> DeleteFileAsync(string containerName, string fileName)
        {
            if (_storageContext.IsAzureBlobAvailable)
            {
                try
                {
                    var container = _storageContext.GetBlobContainerClient(containerName);
                    var blobClient = container.GetBlobClient(fileName);
                    var response = await blobClient.DeleteIfExistsAsync();
                    return response.Value;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to delete blob {FileName} from container {ContainerName}: {Message}", fileName, containerName, ex.Message);
                }
            }

            if (_mockStorage.TryGetValue(containerName, out var containerDict))
            {
                return containerDict.TryRemove(fileName, out _);
            }

            return true;
        }

        public async Task<string> GetBlobUrlAsync(string containerName, string fileName)
        {
            if (_storageContext.IsAzureBlobAvailable)
            {
                try
                {
                    var container = _storageContext.GetBlobContainerClient(containerName);
                    var blobClient = container.GetBlobClient(fileName);
                    return await Task.FromResult(blobClient.Uri.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error getting blob URL for {FileName}: {Message}", fileName, ex.Message);
                }
            }

            return $"/api/blob/{containerName}/{fileName}";
        }

        public async Task<IEnumerable<string>> ListFilesAsync(string containerName)
        {
            if (_storageContext.IsAzureBlobAvailable)
            {
                try
                {
                    var container = _storageContext.GetBlobContainerClient(containerName);
                    var result = new List<string>();
                    await foreach (var item in container.GetBlobsAsync())
                    {
                        result.Add(item.Name);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to list blobs in container {ContainerName}: {Message}", containerName, ex.Message);
                }
            }

            if (_mockStorage.TryGetValue(containerName, out var containerDict))
            {
                return containerDict.Keys.ToList();
            }

            return Enumerable.Empty<string>();
        }

        public async Task<byte[]> GenerateExportReportAsync(string exportType, string title, byte[] data)
        {
            var fileName = $"{exportType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            using var ms = new MemoryStream(data);
            await UploadFileAsync("exports", fileName, ms, "text/csv");
            return data;
        }
    }
}
