namespace MicrosoftPracticeManagement.Data.BlobStorage
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(string containerName, string fileName, Stream content, string contentType);
        Task<Stream?> DownloadFileAsync(string containerName, string fileName);
        Task<bool> DeleteFileAsync(string containerName, string fileName);
        Task<string> GetBlobUrlAsync(string containerName, string fileName);
        Task<IEnumerable<string>> ListFilesAsync(string containerName);
        Task<byte[]> GenerateExportReportAsync(string exportType, string title, byte[] data);
    }
}
