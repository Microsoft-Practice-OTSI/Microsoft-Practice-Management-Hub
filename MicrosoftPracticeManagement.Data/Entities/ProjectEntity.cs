using Azure;
using Azure.Data.Tables;

namespace MicrosoftPracticeManagement.Data.Entities
{
    public class ProjectEntity : ITableEntity
    {
        // Table Storage keys
        public string PartitionKey { get; set; } = string.Empty; // Account (e.g. "Microsoft", "Fabrikam", "Contoso")
        public string RowKey { get; set; } = string.Empty;       // ProjectId (e.g. "PRJ-101")

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Properties
        public string ProjectId => RowKey;
        public string Account => PartitionKey;
        public string ProjectName { get; set; } = string.Empty;
        public string PM { get; set; } = string.Empty; // Project Manager
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(6);
        public string Status { get; set; } = "Active"; // "Active", "Pipeline", "Completed", "On Hold"
        public int StaffingGap { get; set; } = 0; // Number of open positions needed
        public string Description { get; set; } = string.Empty;
        public int TotalAllocated { get; set; } = 0;
        public int RequiredResources { get; set; } = 0;
        public string Health { get; set; } = "Green"; // "Green", "Amber", "Red"
        public string TechnologyStack { get; set; } = string.Empty; // e.g. ".NET 8, Azure, React"
        public decimal Budget { get; set; } = 0;
        public string PracticeArea { get; set; } = "Azure & Cloud Practice";
    }
}
