using Microsoft.Extensions.Logging;
using MicrosoftPracticeManagement.Data.Entities;
using MicrosoftPracticeManagement.Data.Repositories;
using MicrosoftPracticeManagement.Services.DTOs;
using MicrosoftPracticeManagement.Services.Interfaces;

namespace MicrosoftPracticeManagement.Services.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IAllocationRepository _allocationRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ITimesheetRepository _timesheetRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IResourceRepository resourceRepository,
            IProjectRepository projectRepository,
            IAllocationRepository allocationRepository,
            ISkillRepository skillRepository,
            ITimesheetRepository timesheetRepository,
            ILogger<DashboardService> logger)
        {
            _resourceRepository = resourceRepository;
            _projectRepository = projectRepository;
            _allocationRepository = allocationRepository;
            _skillRepository = skillRepository;
            _timesheetRepository = timesheetRepository;
            _logger = logger;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var resources = (await _resourceRepository.GetAllAsync()).ToList();
            var projects = (await _projectRepository.GetAllAsync()).ToList();
            var allocations = (await _allocationRepository.GetAllAsync()).ToList();
            var skills = (await _skillRepository.GetAllAsync()).ToList();
            var currentMonth = DateTime.UtcNow.ToString("yyyy-MM");
            var timesheets = (await _timesheetRepository.GetByYearMonthAsync(currentMonth)).ToList();

            var totalCount = resources.Count > 0 ? resources.Count : 1;
            var billableList = resources.Where(r => r.Billable).ToList();
            var nonBillableList = resources.Where(r => !r.Billable).ToList();
            var benchList = resources.Where(r => r.AllocationPercent == 0 || r.Availability == "Available").ToList();

            var avgExp = resources.Count > 0 ? Math.Round(resources.Average(r => r.ExperienceYears), 1) : 0;
            var avgUtil = billableList.Count > 0 ? Math.Round(billableList.Average(r => r.AllocationPercent), 1) : 0;

            var activeProjects = projects.Where(p => p.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)).ToList();
            var activeAccountsCount = activeProjects.Select(p => p.Account).Distinct().Count();

            // Calculate Practice Health Score (0 - 100)
            double utilScore = Math.Min(35.0, (avgUtil / 85.0) * 35.0);
            double billScore = ((double)billableList.Count / totalCount) * 25.0;
            double benchRatio = (double)benchList.Count / totalCount;
            double benchScore = Math.Max(0.0, (1.0 - (benchRatio * 2.5))) * 20.0;
            var certifiedCount = skills.Count(s => !string.IsNullOrEmpty(s.Certification));
            double skillScore = Math.Min(20.0, ((double)certifiedCount / (totalCount * 1.2)) * 20.0);

            int healthScore = (int)Math.Round(utilScore + billScore + benchScore + skillScore);
            healthScore = Math.Clamp(healthScore, 45, 98);

            string healthStatus = healthScore >= 80 ? "Healthy" : healthScore >= 65 ? "Attention" : "Critical";

            // Attention Required calculations
            var benchOver30 = resources
                .Where(r => (r.AllocationPercent == 0 || r.Availability == "Available") && r.BenchStartDate.HasValue && (DateTime.UtcNow - r.BenchStartDate.Value).TotalDays >= 30)
                .Select(MapToResourceDto)
                .ToList();

            var now = DateTime.UtcNow;
            var in30Days = now.AddDays(30);
            var expiringAllocations = allocations
                .Where(a => a.EndDate >= now && a.EndDate <= in30Days)
                .Select(a => new AllocationDto
                {
                    ProjectId = a.ProjectId,
                    EmployeeId = a.EmployeeId,
                    ProjectName = a.ProjectName,
                    Account = a.Account,
                    EmployeeName = a.EmployeeName,
                    Role = a.Role,
                    AllocationPercent = a.AllocationPercent,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Billable = a.Billable
                })
                .ToList();

            var staffingGaps = activeProjects
                .Where(p => p.StaffingGap > 0)
                .Select(p => new ProjectDto
                {
                    ProjectId = p.ProjectId,
                    Account = p.Account,
                    ProjectName = p.ProjectName,
                    PM = p.PM,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status,
                    StaffingGap = p.StaffingGap,
                    TotalAllocated = p.TotalAllocated,
                    RequiredResources = p.RequiredResources,
                    Health = p.Health,
                    TechnologyStack = p.TechnologyStack
                })
                .ToList();

            var lateTimesheets = timesheets
                .Where(t => t.Status == "Late" || t.Status == "Pending Approval")
                .Select(t => new TimesheetDto
                {
                    YearMonth = t.YearMonth,
                    EmployeeId = t.EmployeeId,
                    EmployeeName = t.EmployeeName,
                    ProjectName = t.ProjectName,
                    HoursLogged = t.HoursLogged,
                    ExpectedHours = t.ExpectedHours,
                    Status = t.Status,
                    SubmissionDate = t.SubmissionDate,
                    ApproverName = t.ApproverName
                })
                .ToList();

            var expiringCerts = skills
                .Where(s => !string.IsNullOrEmpty(s.Certification) && s.ExpiryDate.HasValue && s.ExpiryDate.Value <= now.AddDays(60) && s.ExpiryDate.Value >= now)
                .Select(s => new SkillDto
                {
                    EmployeeId = s.EmployeeId,
                    SkillName = s.SkillName,
                    EmployeeName = s.EmployeeName,
                    Category = s.Category,
                    Level = s.Level,
                    Certification = s.Certification,
                    ExpiryDate = s.ExpiryDate
                })
                .ToList();

            // Chart Payloads
            // 1. Donut: Resource Utilization Breakdown
            int fullyAllocated = billableList.Count(r => r.AllocationPercent >= 100);
            int partiallyAllocated = billableList.Count(r => r.AllocationPercent > 0 && r.AllocationPercent < 100);
            int benchCount = benchList.Count;
            int nonBillableCount = nonBillableList.Count;

            var donutChart = new ChartDataDto
            {
                Labels = new List<string> { "Fully Allocated (100%)", "Partially Allocated", "Bench / Available", "Internal / Non-Billable" },
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Resources",
                        Data = new List<double> { fullyAllocated, partiallyAllocated, benchCount, nonBillableCount },
                        BackgroundColors = new List<string> { "#107c41", "#0078d4", "#ffaa44", "#8a8886" },
                        BorderColors = new List<string> { "#ffffff", "#ffffff", "#ffffff", "#ffffff" },
                        BorderWidth = 2
                    }
                }
            };

            // 2. Bar: Technology Distribution
            var techGroups = new Dictionary<string, int>
            {
                { ".NET 8 / C#", resources.Count(r => r.PrimarySkill.Contains(".NET") || r.PrimarySkill.Contains("C#")) },
                { "Azure Cloud", resources.Count(r => r.PrimarySkill.Contains("Azure") || r.PrimarySkill.Contains("Microservices")) },
                { "AI / OpenAI", resources.Count(r => r.PrimarySkill.Contains("AI") || r.PrimarySkill.Contains("OpenAI") || r.PrimarySkill.Contains("Cognitive")) },
                { "Power Platform", resources.Count(r => r.PrimarySkill.Contains("Power")) },
                { "DevOps / AKS", resources.Count(r => r.PrimarySkill.Contains("DevOps") || r.PrimarySkill.Contains("Kubernetes")) },
                { "Data & Fabric", resources.Count(r => r.PrimarySkill.Contains("Data") || r.PrimarySkill.Contains("Fabric") || r.PrimarySkill.Contains("Databricks")) },
                { "React & Web", resources.Count(r => r.PrimarySkill.Contains("React") || r.PrimarySkill.Contains("Frontend")) }
            };

            var barChart = new ChartDataDto
            {
                Labels = techGroups.Keys.ToList(),
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Headcount",
                        Data = techGroups.Values.Select(v => (double)v).ToList(),
                        BackgroundColors = new List<string> { "#0078d4", "#00bcf2", "#5c2d91", "#008272", "#107c41", "#d83b01", "#b4009e" },
                        BorderColors = new List<string> { "#005a9e", "#0099bc", "#401b6c", "#005e52", "#0b5a2f", "#a80000", "#860074" },
                        BorderWidth = 1
                    }
                }
            };

            // 3. Stacked Bar: Capacity vs Demand
            var techStreams = new[] { ".NET Cloud", "Azure Infra", "AI & Copilot", "Power Platform", "Data & Fabric", "DevOps/SRE" };
            var capacityValues = new List<double> { 58, 46, 32, 28, 38, 30 };
            var demandValues = new List<double> { 52, 49, 36, 25, 42, 28 };

            var capacityDemandChart = new ChartDataDto
            {
                Labels = techStreams.ToList(),
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Available Capacity (Headcount)",
                        Data = capacityValues,
                        BackgroundColors = new List<string> { "#0078d4", "#0078d4", "#0078d4", "#0078d4", "#0078d4", "#0078d4" },
                        Stack = "Capacity"
                    },
                    new ChartDatasetDto
                    {
                        Label = "Active Project Demand (Headcount)",
                        Data = demandValues,
                        BackgroundColors = new List<string> { "#107c41", "#107c41", "#107c41", "#107c41", "#107c41", "#107c41" },
                        Stack = "Demand"
                    }
                }
            };

            // 4. Line: 12-Month Utilization Trend
            var months12 = new List<string> { "Sep", "Oct", "Nov", "Dec", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug" };
            var util12 = new List<double> { 79.4, 80.8, 82.1, 78.5, 81.2, 82.9, 84.0, 83.5, 85.2, 86.1, 84.8, 87.4 };

            var trendChart = new ChartDataDto
            {
                Labels = months12,
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Average Utilization %",
                        Data = util12,
                        BackgroundColors = new List<string> { "rgba(0, 120, 212, 0.15)" },
                        BorderColors = new List<string> { "#0078d4" },
                        BorderWidth = 3,
                        Fill = true,
                        Tension = "0.35"
                    }
                }
            };

            return new DashboardSummaryDto
            {
                TotalResources = resources.Count,
                BillableResources = billableList.Count,
                NonBillableResources = nonBillableList.Count,
                BenchResources = benchList.Count,
                AverageExperienceYears = avgExp,
                AverageUtilizationPercent = avgUtil,
                ActiveProjects = activeProjects.Count,
                ActiveAccounts = activeAccountsCount,
                PracticeHealthScore = healthScore,
                HealthStatus = healthStatus,
                UtilizationFactor = Math.Round(utilScore, 1),
                BillabilityFactor = Math.Round(billScore, 1),
                BenchRatioFactor = Math.Round(benchScore, 1),
                SkillReadinessFactor = Math.Round(skillScore, 1),
                BenchOver30Days = benchOver30,
                ExpiringAllocationsIn30Days = expiringAllocations,
                ProjectsWithStaffingGaps = staffingGaps,
                MissingOrLateTimesheets = lateTimesheets,
                ExpiringCertificationsIn60Days = expiringCerts,
                UtilizationDonutChart = donutChart,
                TechnologyDistributionBarChart = barChart,
                CapacityVsDemandChart = capacityDemandChart,
                UtilizationTrendLineChart = trendChart
            };
        }

        private static ResourceDto MapToResourceDto(ResourceEntity entity)
        {
            return new ResourceDto
            {
                EmployeeId = entity.RowKey,
                ManagerId = entity.PartitionKey,
                ManagerName = entity.ManagerName,
                Name = entity.Name,
                Email = entity.Email,
                Designation = entity.Designation,
                PrimarySkill = entity.PrimarySkill,
                SecondarySkill = entity.SecondarySkill,
                AllocationPercent = entity.AllocationPercent,
                Billable = entity.Billable,
                ExperienceYears = entity.ExperienceYears,
                Location = entity.Location,
                Availability = entity.Availability,
                Department = entity.Department,
                Status = entity.Status,
                HireDate = entity.HireDate,
                BenchStartDate = entity.BenchStartDate,
                ResumeBlobUrl = entity.ResumeBlobUrl,
                ProfileImageUrl = entity.ProfileImageUrl
            };
        }
    }
}
