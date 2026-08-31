using Azure;
using Azure.Data.Tables;

namespace MicrosoftPracticeManagement.Data.Entities
{
    public class LeaveEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty; // YearMonth (e.g. "2026-08")
        public string RowKey { get; set; } = string.Empty;       // LeaveId (e.g. "LV-2026-001")

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string LeaveId => RowKey;
        public string YearMonth => PartitionKey;
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(2);
        public int DaysCount { get; set; } = 2;
        public string LeaveType { get; set; } = "Annual Leave"; // "Annual Leave", "Sick Leave", "Maternity/Paternity", "Bereavement"
        public string Status { get; set; } = "Approved"; // "Approved", "Pending", "Rejected"
        public string Reason { get; set; } = string.Empty;
    }
}
