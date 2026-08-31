using Azure;
using Azure.Data.Tables;

namespace MicrosoftPracticeManagement.Data.Entities
{
    public class AllocationEntity : ITableEntity
    {
        // Table Storage keys
        public string PartitionKey { get; set; } = string.Empty; // ProjectId
        public string RowKey { get; set; } = string.Empty;       // EmployeeId

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Properties
        public string ProjectId => PartitionKey;
        public string EmployeeId => RowKey;
        public string ProjectName { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int AllocationPercent { get; set; } = 100;
        public string Role { get; set; } = string.Empty; // "Lead Developer", "Solution Architect", "DevOps Engineer", etc.
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(3);
        public bool Billable { get; set; } = true;
        public string Notes { get; set; } = string.Empty;
    }
}
