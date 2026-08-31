using Azure;
using Azure.Data.Tables;

namespace MicrosoftPracticeManagement.Data.Entities
{
    public class ResourceEntity : ITableEntity
    {
        // Table Storage keys
        public string PartitionKey { get; set; } = string.Empty; // ManagerId
        public string RowKey { get; set; } = string.Empty;       // EmployeeId

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Properties
        public string EmployeeId => RowKey;
        public string ManagerId => PartitionKey;
        public string ManagerName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string PrimarySkill { get; set; } = string.Empty;
        public string SecondarySkill { get; set; } = string.Empty;
        public int AllocationPercent { get; set; } = 0;
        public bool Billable { get; set; } = true;
        public double ExperienceYears { get; set; } = 0.0;
        public string Location { get; set; } = string.Empty;
        public string Availability { get; set; } = "Available"; // "Available", "Allocated", "Partially Allocated", "On Leave"
        public string Department { get; set; } = "Microsoft Cloud & AI";
        public string Status { get; set; } = "Active"; // "Active", "Notice Period", "Sabbatical"
        public DateTime? HireDate { get; set; }
        public DateTime? BenchStartDate { get; set; }
        public string ResumeBlobUrl { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
    }
}
