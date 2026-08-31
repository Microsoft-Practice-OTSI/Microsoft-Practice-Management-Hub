namespace MicrosoftPracticeManagement.Services.DTOs
{
    public class ResourceDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string ManagerId { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string PrimarySkill { get; set; } = string.Empty;
        public string SecondarySkill { get; set; } = string.Empty;
        public int AllocationPercent { get; set; }
        public bool Billable { get; set; }
        public double ExperienceYears { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Availability { get; set; } = "Available";
        public string Department { get; set; } = "Microsoft Cloud & AI";
        public string Status { get; set; } = "Active";
        public DateTime? HireDate { get; set; }
        public DateTime? BenchStartDate { get; set; }
        public int BenchDays => BenchStartDate.HasValue ? (int)(DateTime.UtcNow - BenchStartDate.Value).TotalDays : 0;
        public string ResumeBlobUrl { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public List<AllocationDto> CurrentAllocations { get; set; } = new();
        public List<SkillDto> Skills { get; set; } = new();
    }

    public class ProjectDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string PM { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Active";
        public int StaffingGap { get; set; }
        public string Description { get; set; } = string.Empty;
        public int TotalAllocated { get; set; }
        public int RequiredResources { get; set; }
        public string Health { get; set; } = "Green";
        public string TechnologyStack { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public string PracticeArea { get; set; } = "Azure & Cloud Practice";
        public List<AllocationDto> Allocations { get; set; } = new();
    }

    public class AllocationDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int AllocationPercent { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Billable { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class SkillDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string SkillName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Level { get; set; } = "Intermediate"; // Beginner, Intermediate, Advanced, Expert
        public string Certification { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public double YearsOfExperience { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.UtcNow.AddDays(60) && ExpiryDate.Value >= DateTime.UtcNow;
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    }
}
