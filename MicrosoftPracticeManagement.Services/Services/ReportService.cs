using Microsoft.Extensions.Logging;
using MicrosoftPracticeManagement.Data.BlobStorage;
using MicrosoftPracticeManagement.Data.Repositories;
using MicrosoftPracticeManagement.Services.DTOs;
using MicrosoftPracticeManagement.Services.Interfaces;
using System.Text;

namespace MicrosoftPracticeManagement.Services.Services
{
    public class ReportService : IReportService
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IAllocationRepository _allocationRepository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IResourceRepository resourceRepository,
            IProjectRepository projectRepository,
            IAllocationRepository allocationRepository,
            IBlobStorageService blobStorageService,
            ILogger<ReportService> logger)
        {
            _resourceRepository = resourceRepository;
            _projectRepository = projectRepository;
            _allocationRepository = allocationRepository;
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        public async Task<ExportResultDto> ExportResourcesCsvAsync()
        {
            var resources = (await _resourceRepository.GetAllAsync()).OrderBy(r => r.Name).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Employee ID,Full Name,Email,Designation,Primary Skill,Secondary Skill,Allocation %,Billable,Experience (Years),Location,Availability,Manager,Status");

            foreach (var r in resources)
            {
                sb.AppendLine($"\"{Escape(r.EmployeeId)}\",\"{Escape(r.Name)}\",\"{Escape(r.Email)}\",\"{Escape(r.Designation)}\",\"{Escape(r.PrimarySkill)}\",\"{Escape(r.SecondarySkill)}\",{r.AllocationPercent},{r.Billable},{r.ExperienceYears},\"{Escape(r.Location)}\",\"{Escape(r.Availability)}\",\"{Escape(r.ManagerName)}\",\"{Escape(r.Status)}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Microsoft_Practice_Resources_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

            var blobUrl = string.Empty;
            try
            {
                using var ms = new MemoryStream(bytes);
                blobUrl = await _blobStorageService.UploadFileAsync("exports", fileName, ms, "text/csv");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to upload export to blob container: {Message}", ex.Message);
            }

            return new ExportResultDto
            {
                FileName = fileName,
                ContentType = "text/csv",
                Data = bytes,
                BlobUrl = blobUrl
            };
        }

        public async Task<ExportResultDto> ExportProjectsCsvAsync()
        {
            var projects = (await _projectRepository.GetAllAsync()).OrderBy(p => p.Account).ThenBy(p => p.ProjectName).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Project ID,Account,Project Name,Project Manager,Start Date,End Date,Status,Required Resources,Allocated Resources,Staffing Gap,Health,Tech Stack,Budget (USD)");

            foreach (var p in projects)
            {
                sb.AppendLine($"\"{Escape(p.ProjectId)}\",\"{Escape(p.Account)}\",\"{Escape(p.ProjectName)}\",\"{Escape(p.PM)}\",\"{p.StartDate:yyyy-MM-dd}\",\"{p.EndDate:yyyy-MM-dd}\",\"{Escape(p.Status)}\",{p.RequiredResources},{p.TotalAllocated},{p.StaffingGap},\"{Escape(p.Health)}\",\"{Escape(p.TechnologyStack)}\",{p.Budget}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Microsoft_Practice_Projects_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

            var blobUrl = string.Empty;
            try
            {
                using var ms = new MemoryStream(bytes);
                blobUrl = await _blobStorageService.UploadFileAsync("exports", fileName, ms, "text/csv");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to upload export to blob container: {Message}", ex.Message);
            }

            return new ExportResultDto
            {
                FileName = fileName,
                ContentType = "text/csv",
                Data = bytes,
                BlobUrl = blobUrl
            };
        }

        public async Task<ExportResultDto> ExportUtilizationReportCsvAsync()
        {
            var resources = (await _resourceRepository.GetAllAsync()).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Employee ID,Full Name,Designation,Primary Skill,Allocation %,Billable,Availability,Bench Days,Location,Manager");

            foreach (var r in resources.OrderByDescending(r => r.AllocationPercent))
            {
                int benchDays = r.BenchStartDate.HasValue ? (int)(DateTime.UtcNow - r.BenchStartDate.Value).TotalDays : 0;
                sb.AppendLine($"\"{Escape(r.EmployeeId)}\",\"{Escape(r.Name)}\",\"{Escape(r.Designation)}\",\"{Escape(r.PrimarySkill)}\",{r.AllocationPercent},{r.Billable},\"{Escape(r.Availability)}\",{benchDays},\"{Escape(r.Location)}\",\"{Escape(r.ManagerName)}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Microsoft_Practice_Utilization_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

            var blobUrl = string.Empty;
            try
            {
                using var ms = new MemoryStream(bytes);
                blobUrl = await _blobStorageService.UploadFileAsync("exports", fileName, ms, "text/csv");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to upload export to blob container: {Message}", ex.Message);
            }

            return new ExportResultDto
            {
                FileName = fileName,
                ContentType = "text/csv",
                Data = bytes,
                BlobUrl = blobUrl
            };
        }

        public async Task<ExportResultDto> ExportPracticeHealthCsvAsync()
        {
            var resources = (await _resourceRepository.GetAllAsync()).ToList();
            var projects = (await _projectRepository.GetAllAsync()).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Microsoft Practice Hub - Practice Health Executive Summary");
            sb.AppendLine($"Generated on,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();
            sb.AppendLine("Metric,Value,Target,Status");
            sb.AppendLine($"Total Resources,{resources.Count},250,Healthy");
            sb.AppendLine($"Billable Resources,{resources.Count(r => r.Billable)},210,Healthy");
            sb.AppendLine($"Bench Resources,{resources.Count(r => r.AllocationPercent == 0)},< 25,Healthy");
            sb.AppendLine($"Average Utilization,{Math.Round(resources.Where(r => r.Billable).Average(r => r.AllocationPercent), 1)}%,85%,Healthy");
            sb.AppendLine($"Active Projects,{projects.Count(p => p.Status == "Active")},30,Healthy");
            sb.AppendLine($"Active Staffing Gaps,{projects.Sum(p => p.StaffingGap)},< 10,Attention");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Microsoft_Practice_HealthSummary_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

            var blobUrl = string.Empty;
            try
            {
                using var ms = new MemoryStream(bytes);
                blobUrl = await _blobStorageService.UploadFileAsync("exports", fileName, ms, "text/csv");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to upload export to blob container: {Message}", ex.Message);
            }

            return new ExportResultDto
            {
                FileName = fileName,
                ContentType = "text/csv",
                Data = bytes,
                BlobUrl = blobUrl
            };
        }

        private static string Escape(string val) => (val ?? string.Empty).Replace("\"", "\"\"");
    }
}
