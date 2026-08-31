namespace MicrosoftPracticeManagement.Data.Storage
{
    public class StorageConfiguration
    {
        public string TableStorageConnection { get; set; } = "UseDevelopmentStorage=true";
        public string BlobStorageConnection { get; set; } = "UseDevelopmentStorage=true";
        public string StorageAccountName { get; set; } = "devstoreaccount1";
        public string BlobContainerPrefix { get; set; } = "mph";
        public bool UseInMemoryFallbackIfUnavailable { get; set; } = true;
    }
}
