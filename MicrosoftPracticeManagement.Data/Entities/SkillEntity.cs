using Azure;
using Azure.Data.Tables;

namespace MicrosoftPracticeManagement.Data.Entities
{
    public class SkillEntity : ITableEntity
    {
        // Table Storage keys
        public string PartitionKey { get; set; } = string.Empty; // EmployeeId
        public string RowKey { get; set; } = string.Empty;       // SkillName (e.g. ".NET 8", "Azure DevOps", "Kubernetes")

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Properties
        public string EmployeeId => PartitionKey;
        public string SkillName => RowKey;
        public string EmployeeName { get; set; } = string.Empty;
        public string Category { get; set; } = "Cloud & Backend"; // Cloud, AI/ML, DevOps, Frontend, Data, Power Platform
        public string Level { get; set; } = "Intermediate"; // "Beginner", "Intermediate", "Advanced", "Expert"
        public string Certification { get; set; } = string.Empty; // e.g. "AZ-204", "AZ-400", "AI-102"
        public string CertificationBadgeUrl { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public double YearsOfExperience { get; set; } = 2.0;
        public bool IsPrimary { get; set; } = false;
    }
}
