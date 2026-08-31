using MicrosoftPracticeManagement.Services.DTOs;

namespace MicrosoftPracticeManagement.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    }

    public interface IResourceService
    {
        Task<IEnumerable<ResourceDto>> GetAllResourcesAsync();
        Task<(IEnumerable<ResourceDto> Items, int TotalCount)> GetFilteredResourcesAsync(
            string? search, string? account, string? project, string? technology, 
            string? location, string? designation, string? status, string? availability,
            string? sortBy, bool sortDesc, int pageIndex, int pageSize);
        Task<ResourceDto?> GetResourceByIdAsync(string employeeId);
        Task<ResourceDto?> GetResourceByManagerAndIdAsync(string managerId, string employeeId);
        Task AddResourceAsync(ResourceDto resource);
        Task UpdateResourceAsync(ResourceDto resource);
        Task DeleteResourceAsync(string managerId, string employeeId);
        Task<Stream?> DownloadResumeAsync(string employeeId);
    }

    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
        Task<IEnumerable<ProjectDto>> GetFilteredProjectsAsync(string? search, string? account, string? status, string? health);
        Task<ProjectDto?> GetProjectByIdAsync(string account, string projectId);
        Task<GanttTimelineDto> GetGanttTimelineAsync();
        Task AddProjectAsync(ProjectDto project);
        Task UpdateProjectAsync(ProjectDto project);
    }

    public interface ISkillService
    {
        Task<SkillsMatrixDto> GetSkillsMatrixAsync(string? techFilter, string? levelFilter);
        Task<IEnumerable<SkillDto>> GetExpiringCertificationsAsync(int daysAhead = 60);
        Task<IEnumerable<MissingSkillGapDto>> GetMissingSkillsReportAsync();
    }

    public interface IPracticeHealthService
    {
        Task<PracticeHealthDto> GetPracticeHealthMetricsAsync();
    }

    public interface ITimesheetService
    {
        Task<TimesheetComplianceDto> GetComplianceDashboardAsync(string? yearMonth);
        Task SubmitTimesheetAsync(TimesheetDto timesheet);
        Task ApproveTimesheetAsync(string yearMonth, string employeeId, string approverName);
    }

    public interface ILeaveService
    {
        Task<LeaveSummaryDto> GetLeaveSummaryAsync(string? yearMonth);
        Task RequestLeaveAsync(LeaveDto leave);
    }

    public interface IAppraisalService
    {
        Task<AppraisalSummaryDto> GetAppraisalCycleSummaryAsync(string? cycleYear);
    }

    public interface IInnovationService
    {
        Task<IEnumerable<InnovationDto>> GetAllAssetsAsync(string? category);
        Task<InnovationDto?> GetAssetByIdAsync(string category, string id);
        Task<Stream?> DownloadAssetDocumentAsync(string category, string id);
    }

    public interface IReportService
    {
        Task<ExportResultDto> ExportResourcesCsvAsync();
        Task<ExportResultDto> ExportProjectsCsvAsync();
        Task<ExportResultDto> ExportUtilizationReportCsvAsync();
        Task<ExportResultDto> ExportPracticeHealthCsvAsync();
    }
}
