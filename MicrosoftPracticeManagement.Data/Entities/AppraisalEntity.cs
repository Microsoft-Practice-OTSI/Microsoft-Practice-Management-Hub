using Azure;
using Azure.Data.Tables;

namespace MicrosoftPracticeManagement.Data.Entities
{
    public class AppraisalEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty; // CycleYear (e.g. "FY26-Q1", "FY26-Annual")
        public string RowKey { get; set; } = string.Empty;       // EmployeeId (e.g. "EMP-1001")

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string CycleYear => PartitionKey;
        public string EmployeeId => RowKey;
        public string EmployeeName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public decimal PerformanceRating { get; set; } = 4.5m; // 1.0 to 5.0
        public string PromotionReadiness { get; set; } = "Ready Now"; // "Ready Now", "Ready in 1 Year", "Developing", "Not Applicable"
        public string TargetDesignation { get; set; } = string.Empty;
        public string ReviewStatus { get; set; } = "Completed"; // "Self-Review", "Manager Review", "Leadership Review", "Completed"
        public string KeyStrengths { get; set; } = string.Empty;
        public string DevelopmentAreas { get; set; } = string.Empty;
        public string FeedbackSummary { get; set; } = string.Empty;
    }
}
