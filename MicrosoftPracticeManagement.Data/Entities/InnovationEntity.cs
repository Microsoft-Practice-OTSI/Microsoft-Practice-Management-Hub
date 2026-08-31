using Azure;
using Azure.Data.Tables;

namespace MicrosoftPracticeManagement.Data.Entities
{
    public class InnovationEntity : ITableEntity
    {
        // Table Storage keys
        public string PartitionKey { get; set; } = string.Empty; // Category: "CaseStudy", "Accelerator", "ArchitecturePattern"
        public string RowKey { get; set; } = string.Empty;       // AssetId (e.g. "AST-001")

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string Id => RowKey;
        public string Category => PartitionKey;
        public string Title { get; set; } = string.Empty;
        public string ClientAccount { get; set; } = string.Empty;
        public string BusinessProblem { get; set; } = string.Empty;
        public string Solution { get; set; } = string.Empty;
        public string BusinessValue { get; set; } = string.Empty;
        public string TechnologyTags { get; set; } = string.Empty; // e.g. "Azure OpenAI, .NET 8, Cosmos DB"
        public string SharePointLink { get; set; } = "https://microsoft.sharepoint.com/teams/PracticeHub";
        public string DocumentBlobUrl { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorRole { get; set; } = string.Empty;
        public int DownloadsCount { get; set; } = 0;
        public int RatingStars { get; set; } = 5;
        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
    }
}
