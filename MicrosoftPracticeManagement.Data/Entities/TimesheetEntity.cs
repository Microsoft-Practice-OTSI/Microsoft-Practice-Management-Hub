using Azure;
using Azure.Data.Tables;

namespace MicrosoftPracticeManagement.Data.Entities
{
    public class TimesheetEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty; // YearMonth (e.g. "2026-08")
        public string RowKey { get; set; } = string.Empty;       // EmployeeId (e.g. "EMP-1001")

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string YearMonth => PartitionKey;
        public string EmployeeId => RowKey;
        public string EmployeeName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public decimal HoursLogged { get; set; } = 40;
        public decimal ExpectedHours { get; set; } = 40;
        public string Status { get; set; } = "Approved"; // "Approved", "Submitted", "Late", "Pending Approval", "Missing"
        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
        public string ApproverName { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }
}
