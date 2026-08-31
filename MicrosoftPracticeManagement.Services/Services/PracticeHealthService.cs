using Microsoft.Extensions.Logging;
using MicrosoftPracticeManagement.Data.Entities;
using MicrosoftPracticeManagement.Data.Repositories;
using MicrosoftPracticeManagement.Services.DTOs;
using MicrosoftPracticeManagement.Services.Interfaces;

namespace MicrosoftPracticeManagement.Services.Services
{
    public class PracticeHealthService : IPracticeHealthService
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ILogger<PracticeHealthService> _logger;

        public PracticeHealthService(
            IResourceRepository resourceRepository,
            IProjectRepository projectRepository,
            ISkillRepository skillRepository,
            ILogger<PracticeHealthService> logger)
        {
            _resourceRepository = resourceRepository;
            _projectRepository = projectRepository;
            _skillRepository = skillRepository;
            _logger = logger;
        }

        public async Task<PracticeHealthDto> GetPracticeHealthMetricsAsync()
        {
            var resources = (await _resourceRepository.GetAllAsync()).ToList();
            var totalCount = resources.Count > 0 ? resources.Count : 1;

            // 1. 12-Month Utilization Trend
            var months = new List<string> { "Oct", "Nov", "Dec", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep" };
            var utilData = new List<double> { 80.2, 81.5, 78.9, 82.3, 83.1, 84.6, 85.0, 84.2, 86.8, 87.1, 85.9, 88.2 };

            var utilTrendChart = new ChartDataDto
            {
                Labels = months,
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Target Utilization (85%)",
                        Data = new List<double> { 85, 85, 85, 85, 85, 85, 85, 85, 85, 85, 85, 85 },
                        BorderColors = new List<string> { "#d83b01" },
                        BorderWidth = 2,
                        Fill = false,
                        Tension = "0"
                    },
                    new ChartDatasetDto
                    {
                        Label = "Actual Practice Utilization %",
                        Data = utilData,
                        BackgroundColors = new List<string> { "rgba(0, 120, 212, 0.15)" },
                        BorderColors = new List<string> { "#0078d4" },
                        BorderWidth = 3,
                        Fill = true,
                        Tension = "0.35"
                    }
                }
            };

            // 2. Billability Trend Monthly
            var billabilityData = new List<double> { 81.0, 82.2, 80.5, 83.0, 84.5, 85.8, 86.2, 85.5, 87.0, 88.4, 87.6, 89.1 };
            var billabilityChart = new ChartDataDto
            {
                Labels = months,
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Billability %",
                        Data = billabilityData,
                        BackgroundColors = new List<string> { "rgba(16, 124, 65, 0.15)" },
                        BorderColors = new List<string> { "#107c41" },
                        BorderWidth = 3,
                        Fill = true,
                        Tension = "0.3"
                    }
                }
            };

            // 3. Experience Distribution Tiers: 0-3 yrs, 3-5 yrs, 5-8 yrs, 8-12 yrs, 12+ yrs
            int exp0_3 = resources.Count(r => r.ExperienceYears < 3.0);
            int exp3_5 = resources.Count(r => r.ExperienceYears >= 3.0 && r.ExperienceYears < 5.0);
            int exp5_8 = resources.Count(r => r.ExperienceYears >= 5.0 && r.ExperienceYears < 8.0);
            int exp8_12 = resources.Count(r => r.ExperienceYears >= 8.0 && r.ExperienceYears < 12.0);
            int exp12_plus = resources.Count(r => r.ExperienceYears >= 12.0);

            var expChart = new ChartDataDto
            {
                Labels = new List<string> { "0-3 Years (Junior)", "3-5 Years (Mid)", "5-8 Years (Senior)", "8-12 Years (Lead)", "12+ Years (Architect/Principal)" },
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Consultants",
                        Data = new List<double> { exp0_3, exp3_5, exp5_8, exp8_12, exp12_plus },
                        BackgroundColors = new List<string> { "#00bcf2", "#0078d4", "#004b87", "#5c2d91", "#008272" },
                        BorderColors = new List<string> { "#0099bc", "#005a9e", "#00325a", "#401b6c", "#005e52" },
                        BorderWidth = 1
                    }
                }
            };

            // 4. Attrition Trend
            var attritionMonths = new List<string> { "Q1 FY25", "Q2 FY25", "Q3 FY25", "Q4 FY25", "Q1 FY26", "Q2 FY26" };
            var attritionRates = new List<double> { 12.4, 11.1, 9.8, 8.5, 7.9, 7.2 };

            var attritionChart = new ChartDataDto
            {
                Labels = attritionMonths,
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Annualized Attrition Rate % (Benchmark < 10%)",
                        Data = attritionRates,
                        BackgroundColors = new List<string> { "rgba(216, 59, 1, 0.15)" },
                        BorderColors = new List<string> { "#d83b01" },
                        BorderWidth = 2,
                        Fill = true,
                        Tension = "0.3"
                    }
                }
            };

            // 5. Future Hiring Requirements
            var hiringReqs = new List<HiringNeedDto>
            {
                new HiringNeedDto { Technology = "Azure OpenAI & Semantic Kernel", Role = "Senior AI Solutions Architect", ExperienceLevel = "8-12 Years", OpenPositions = 5, Priority = "Urgent", TargetQuarter = "Q1 FY26" },
                new HiringNeedDto { Technology = ".NET 8 & Microservices", Role = "Lead Full Stack Engineer", ExperienceLevel = "6-10 Years", OpenPositions = 8, Priority = "High", TargetQuarter = "Q1 FY26" },
                new HiringNeedDto { Technology = "Microsoft Fabric & Databricks", Role = "Senior Data Architect", ExperienceLevel = "8-12 Years", OpenPositions = 4, Priority = "High", TargetQuarter = "Q2 FY26" },
                new HiringNeedDto { Technology = "Azure Kubernetes & Terraform", Role = "DevOps / SRE Specialist", ExperienceLevel = "4-7 Years", OpenPositions = 6, Priority = "Medium", TargetQuarter = "Q2 FY26" },
                new HiringNeedDto { Technology = "Power Platform Governance", Role = "Power Platform Lead Consultant", ExperienceLevel = "5-8 Years", OpenPositions = 3, Priority = "Medium", TargetQuarter = "Q2 FY26" }
            };

            return new PracticeHealthDto
            {
                HealthScore = 88,
                HealthStatus = "Healthy",
                UtilizationTrend12M = utilTrendChart,
                BillabilityTrendMonthly = billabilityChart,
                ExperienceDistribution = expChart,
                AttritionTrend = attritionChart,
                HiringRequirements = hiringReqs
            };
        }
    }
}
