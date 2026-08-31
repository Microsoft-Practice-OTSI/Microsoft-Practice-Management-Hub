using Microsoft.Extensions.Logging;
using MicrosoftPracticeManagement.Data.Entities;
using MicrosoftPracticeManagement.Data.Repositories;
using MicrosoftPracticeManagement.Services.DTOs;
using MicrosoftPracticeManagement.Services.Interfaces;

namespace MicrosoftPracticeManagement.Services.Services
{
    public class SkillService : ISkillService
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<SkillService> _logger;

        public SkillService(
            ISkillRepository skillRepository,
            IProjectRepository projectRepository,
            ILogger<SkillService> logger)
        {
            _skillRepository = skillRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        public async Task<SkillsMatrixDto> GetSkillsMatrixAsync(string? techFilter, string? levelFilter)
        {
            var allSkills = (await _skillRepository.GetAllAsync()).ToList();

            var query = allSkills.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(techFilter))
            {
                query = query.Where(s => s.SkillName.Contains(techFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(levelFilter))
            {
                query = query.Where(s => s.Level.Equals(levelFilter, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = query.ToList();

            var groupedByTech = filtered
                .GroupBy(s => s.SkillName)
                .Select(g => new TechSkillCountDto
                {
                    Technology = g.Key,
                    Category = g.First().Category,
                    BeginnerCount = g.Count(s => s.Level == "Beginner"),
                    IntermediateCount = g.Count(s => s.Level == "Intermediate"),
                    AdvancedCount = g.Count(s => s.Level == "Advanced"),
                    ExpertCount = g.Count(s => s.Level == "Expert"),
                    TotalCertified = g.Count(s => !string.IsNullOrEmpty(s.Certification))
                })
                .OrderByDescending(t => t.TotalExperts)
                .ThenBy(t => t.Technology)
                .ToList();

            var expiringCerts = (await GetExpiringCertificationsAsync(60)).ToList();
            var missingGaps = (await GetMissingSkillsReportAsync()).ToList();

            return new SkillsMatrixDto
            {
                Technologies = groupedByTech.Select(g => g.Technology).ToList(),
                SkillCounts = groupedByTech,
                ExpiringCertifications = expiringCerts,
                MissingSkillsGaps = missingGaps
            };
        }

        public async Task<IEnumerable<SkillDto>> GetExpiringCertificationsAsync(int daysAhead = 60)
        {
            var allSkills = await _skillRepository.GetAllAsync();
            var now = DateTime.UtcNow;
            var target = now.AddDays(daysAhead);

            return allSkills
                .Where(s => !string.IsNullOrEmpty(s.Certification) && s.ExpiryDate.HasValue && s.ExpiryDate.Value <= target && s.ExpiryDate.Value >= now)
                .Select(s => new SkillDto
                {
                    EmployeeId = s.EmployeeId,
                    SkillName = s.SkillName,
                    EmployeeName = s.EmployeeName,
                    Category = s.Category,
                    Level = s.Level,
                    Certification = s.Certification,
                    IssueDate = s.IssueDate,
                    ExpiryDate = s.ExpiryDate,
                    YearsOfExperience = s.YearsOfExperience
                })
                .OrderBy(s => s.ExpiryDate)
                .ToList();
        }

        public async Task<IEnumerable<MissingSkillGapDto>> GetMissingSkillsReportAsync()
        {
            var allSkills = (await _skillRepository.GetAllAsync()).ToList();
            var activeProjects = (await _projectRepository.GetAllAsync()).Where(p => p.Status == "Active").ToList();

            var coreTechs = new[]
            {
                ("Azure OpenAI & AI Search", 18, "High"),
                ("Azure Kubernetes Service (AKS)", 12, "High"),
                ("Microsoft Fabric & Databricks", 14, "Medium"),
                ("Power Platform ALM", 10, "Medium"),
                (".NET 8 Microservices", 22, "High"),
                ("Terraform & Azure Bicep", 8, "Medium")
            };

            var gaps = new List<MissingSkillGapDto>();
            foreach (var (tech, demand, priority) in coreTechs)
            {
                int capacity = allSkills.Count(s => s.SkillName.Contains(tech.Split(' ')[0], StringComparison.OrdinalIgnoreCase) && (s.Level == "Advanced" || s.Level == "Expert"));
                gaps.Add(new MissingSkillGapDto
                {
                    Technology = tech,
                    CurrentCapacity = capacity,
                    ProjectDemand = demand,
                    Priority = priority
                });
            }

            return gaps;
        }
    }
}
