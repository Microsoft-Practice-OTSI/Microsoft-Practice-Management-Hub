using Microsoft.Extensions.Logging;
using MicrosoftPracticeManagement.Data.Entities;
using MicrosoftPracticeManagement.Data.Repositories;

namespace MicrosoftPracticeManagement.Data.Storage
{
    public class DataSeeder
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IAllocationRepository _allocationRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IInnovationRepository _innovationRepository;
        private readonly ITimesheetRepository _timesheetRepository;
        private readonly ILeaveRepository _leaveRepository;
        private readonly IAppraisalRepository _appraisalRepository;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(
            IResourceRepository resourceRepository,
            IProjectRepository projectRepository,
            IAllocationRepository allocationRepository,
            ISkillRepository skillRepository,
            IInnovationRepository innovationRepository,
            ITimesheetRepository timesheetRepository,
            ILeaveRepository leaveRepository,
            IAppraisalRepository appraisalRepository,
            ILogger<DataSeeder> logger)
        {
            _resourceRepository = resourceRepository;
            _projectRepository = projectRepository;
            _allocationRepository = allocationRepository;
            _skillRepository = skillRepository;
            _innovationRepository = innovationRepository;
            _timesheetRepository = timesheetRepository;
            _leaveRepository = leaveRepository;
            _appraisalRepository = appraisalRepository;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                var existingResources = await _resourceRepository.GetAllAsync();
                if (existingResources.Any())
                {
                    _logger.LogInformation("Data already seeded. Skipping initialization.");
                    return;
                }

                _logger.LogInformation("Data seeding is currently disabled. Add custom data here.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during seed data execution.");
            }
        }
    }
}
