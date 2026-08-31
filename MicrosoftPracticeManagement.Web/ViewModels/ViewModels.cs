using MicrosoftPracticeManagement.Services.DTOs;

namespace MicrosoftPracticeManagement.Web.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardSummaryDto Summary { get; set; } = new();
        public string SelectedPractice { get; set; } = "Microsoft Azure & Cloud Practice";
    }

    public class ResourceListViewModel
    {
        public IEnumerable<ResourceDto> Resources { get; set; } = Enumerable.Empty<ResourceDto>();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Filter parameters
        public string? Search { get; set; }
        public string? Account { get; set; }
        public string? Project { get; set; }
        public string? Technology { get; set; }
        public string? Location { get; set; }
        public string? Designation { get; set; }
        public string? Status { get; set; }
        public string? Availability { get; set; }
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; }

        // Select lists
        public List<string> AvailableLocations { get; set; } = new();
        public List<string> AvailableTechnologies { get; set; } = new();
        public List<string> AvailableDesignations { get; set; } = new();
    }

    public class ResourceDetailsViewModel
    {
        public ResourceDto Resource { get; set; } = new();
    }

    public class ProjectListViewModel
    {
        public IEnumerable<ProjectDto> Projects { get; set; } = Enumerable.Empty<ProjectDto>();
        public string? Search { get; set; }
        public string? Account { get; set; }
        public string? Status { get; set; }
        public string? Health { get; set; }
        public List<string> AvailableAccounts { get; set; } = new();
    }

    public class ProjectDetailsViewModel
    {
        public ProjectDto Project { get; set; } = new();
    }

    public class GanttTimelineViewModel
    {
        public GanttTimelineDto Timeline { get; set; } = new();
    }

    public class SkillsMatrixViewModel
    {
        public SkillsMatrixDto Matrix { get; set; } = new();
        public string? TechFilter { get; set; }
        public string? LevelFilter { get; set; }
    }

    public class PracticeHealthViewModel
    {
        public PracticeHealthDto Health { get; set; } = new();
    }

    public class TimesheetsViewModel
    {
        public TimesheetComplianceDto Compliance { get; set; } = new();
        public string SelectedMonth { get; set; } = DateTime.UtcNow.ToString("yyyy-MM");
    }

    public class LeaveViewModel
    {
        public LeaveSummaryDto Summary { get; set; } = new();
        public string SelectedMonth { get; set; } = DateTime.UtcNow.ToString("yyyy-MM");
    }

    public class AppraisalsViewModel
    {
        public AppraisalSummaryDto Summary { get; set; } = new();
    }

    public class InnovationViewModel
    {
        public IEnumerable<InnovationDto> Assets { get; set; } = Enumerable.Empty<InnovationDto>();
        public string? SelectedCategory { get; set; }
    }

    public class ReportsViewModel
    {
        public string Message { get; set; } = string.Empty;
    }
}
