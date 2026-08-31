namespace MicrosoftPracticeManagement.Services.DTOs
{
    public class DashboardSummaryDto
    {
        // 8 Key KPI Cards
        public int TotalResources { get; set; }
        public int BillableResources { get; set; }
        public int NonBillableResources { get; set; }
        public int BenchResources { get; set; }
        public double AverageExperienceYears { get; set; }
        public double AverageUtilizationPercent { get; set; }
        public int ActiveProjects { get; set; }
        public int ActiveAccounts { get; set; }

        // Trends vs Last Month
        public double UtilizationTrendDiff { get; set; } = +3.2;
        public double BillableTrendDiff { get; set; } = +2.4;
        public int BenchTrendDiff { get; set; } = -4;
        public int ProjectsTrendDiff { get; set; } = +3;

        // Practice Health Score
        public int PracticeHealthScore { get; set; } = 87;
        public string HealthStatus { get; set; } = "Healthy"; // Healthy, Attention, Critical
        public double UtilizationFactor { get; set; }
        public double BillabilityFactor { get; set; }
        public double BenchRatioFactor { get; set; }
        public double SkillReadinessFactor { get; set; }

        // Attention Required Items
        public List<ResourceDto> BenchOver30Days { get; set; } = new();
        public List<AllocationDto> ExpiringAllocationsIn30Days { get; set; } = new();
        public List<TimesheetDto> MissingOrLateTimesheets { get; set; } = new();
        public List<ProjectDto> ProjectsWithStaffingGaps { get; set; } = new();
        public List<SkillDto> ExpiringCertificationsIn60Days { get; set; } = new();

        // Chart Data Payloads
        public ChartDataDto UtilizationDonutChart { get; set; } = new();
        public ChartDataDto TechnologyDistributionBarChart { get; set; } = new();
        public ChartDataDto CapacityVsDemandChart { get; set; } = new();
        public ChartDataDto UtilizationTrendLineChart { get; set; } = new();
    }

    public class ChartDataDto
    {
        public List<string> Labels { get; set; } = new();
        public List<ChartDatasetDto> Datasets { get; set; } = new();
    }

    public class ChartDatasetDto
    {
        public string Label { get; set; } = string.Empty;
        public List<double> Data { get; set; } = new();
        public List<string> BackgroundColors { get; set; } = new();
        public List<string> BorderColors { get; set; } = new();
        public int BorderWidth { get; set; } = 1;
        public bool Fill { get; set; } = false;
        public string Tension { get; set; } = "0.4";
        public string? Stack { get; set; }
    }

    public class GanttTimelineDto
    {
        public List<GanttProjectItemDto> Projects { get; set; } = new();
        public List<ResourceDto> OverallocatedResources { get; set; } = new();
        public List<ProjectDto> UnderallocatedProjects { get; set; } = new();
        public List<ResourceDto> BenchResources { get; set; } = new();
    }

    public class GanttProjectItemDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string PM { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Required { get; set; }
        public int Allocated { get; set; }
        public int Gap { get; set; }
        public string Health { get; set; } = "Green";
        public List<GanttAllocationItemDto> ResourceAllocations { get; set; } = new();
    }

    public class GanttAllocationItemDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int AllocationPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SkillsMatrixDto
    {
        public List<string> Technologies { get; set; } = new();
        public List<TechSkillCountDto> SkillCounts { get; set; } = new();
        public List<SkillDto> ExpiringCertifications { get; set; } = new();
        public List<MissingSkillGapDto> MissingSkillsGaps { get; set; } = new();
    }

    public class TechSkillCountDto
    {
        public string Technology { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int BeginnerCount { get; set; }
        public int IntermediateCount { get; set; }
        public int AdvancedCount { get; set; }
        public int ExpertCount { get; set; }
        public int TotalCertified { get; set; }
        public int TotalExperts => AdvancedCount + ExpertCount;
    }

    public class MissingSkillGapDto
    {
        public string Technology { get; set; } = string.Empty;
        public int CurrentCapacity { get; set; }
        public int ProjectDemand { get; set; }
        public int GapCount => Math.Max(0, ProjectDemand - CurrentCapacity);
        public string Priority { get; set; } = "Medium";
    }

    public class PracticeHealthDto
    {
        public int HealthScore { get; set; }
        public string HealthStatus { get; set; } = "Healthy";
        public ChartDataDto UtilizationTrend12M { get; set; } = new();
        public ChartDataDto BillabilityTrendMonthly { get; set; } = new();
        public ChartDataDto ExperienceDistribution { get; set; } = new();
        public ChartDataDto AttritionTrend { get; set; } = new();
        public List<HiringNeedDto> HiringRequirements { get; set; } = new();
    }

    public class HiringNeedDto
    {
        public string Technology { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ExperienceLevel { get; set; } = string.Empty;
        public int OpenPositions { get; set; }
        public string Priority { get; set; } = "High";
        public string TargetQuarter { get; set; } = "Q1 FY26";
    }

    public class TimesheetDto
    {
        public string YearMonth { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public decimal HoursLogged { get; set; }
        public decimal ExpectedHours { get; set; }
        public string Status { get; set; } = "Submitted";
        public DateTime SubmissionDate { get; set; }
        public string ApproverName { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }

    public class TimesheetComplianceDto
    {
        public decimal ComplianceRatePercent { get; set; }
        public int TotalSubmissions { get; set; }
        public int LateSubmissionsCount { get; set; }
        public int PendingApprovalsCount { get; set; }
        public List<TimesheetDto> RecentTimesheets { get; set; } = new();
    }

    public class LeaveSummaryDto
    {
        public List<LeaveDto> UpcomingLeaves { get; set; } = new();
        public int TotalOnLeaveToday { get; set; }
        public int TotalOnLeaveThisMonth { get; set; }
    }

    public class LeaveDto
    {
        public string LeaveId { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysCount { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public string Status { get; set; } = "Approved";
        public string Reason { get; set; } = string.Empty;
    }

    public class AppraisalSummaryDto
    {
        public string CurrentCycle { get; set; } = "FY26 Annual";
        public int TotalAppraisals { get; set; }
        public int CompletedAppraisals { get; set; }
        public int PromotionReadyCount { get; set; }
        public decimal AverageRating { get; set; }
        public List<AppraisalDto> Appraisals { get; set; } = new();
    }

    public class AppraisalDto
    {
        public string CycleYear { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public decimal PerformanceRating { get; set; }
        public string PromotionReadiness { get; set; } = string.Empty;
        public string TargetDesignation { get; set; } = string.Empty;
        public string ReviewStatus { get; set; } = string.Empty;
        public string KeyStrengths { get; set; } = string.Empty;
        public string DevelopmentAreas { get; set; } = string.Empty;
        public string FeedbackSummary { get; set; } = string.Empty;
    }

    public class InnovationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // CaseStudy, Accelerator
        public string Title { get; set; } = string.Empty;
        public string ClientAccount { get; set; } = string.Empty;
        public string BusinessProblem { get; set; } = string.Empty;
        public string Solution { get; set; } = string.Empty;
        public string BusinessValue { get; set; } = string.Empty;
        public string TechnologyTags { get; set; } = string.Empty;
        public string SharePointLink { get; set; } = string.Empty;
        public string DocumentBlobUrl { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorRole { get; set; } = string.Empty;
        public int DownloadsCount { get; set; }
        public int RatingStars { get; set; }
        public DateTime PublishedDate { get; set; }
    }

    public class ExportResultDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "text/csv";
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string BlobUrl { get; set; } = string.Empty;
    }
}
