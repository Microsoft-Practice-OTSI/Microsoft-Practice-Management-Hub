using Microsoft.Extensions.Logging;
using MicrosoftPracticeManagement.Data.Entities;
using MicrosoftPracticeManagement.Data.Repositories;
using MicrosoftPracticeManagement.Services.DTOs;
using MicrosoftPracticeManagement.Services.Interfaces;

namespace MicrosoftPracticeManagement.Services.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IAllocationRepository _allocationRepository;
        private readonly IResourceRepository _resourceRepository;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(
            IProjectRepository projectRepository,
            IAllocationRepository allocationRepository,
            IResourceRepository resourceRepository,
            ILogger<ProjectService> logger)
        {
            _projectRepository = projectRepository;
            _allocationRepository = allocationRepository;
            _resourceRepository = resourceRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            var entities = await _projectRepository.GetAllAsync();
            return entities.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<ProjectDto>> GetFilteredProjectsAsync(string? search, string? account, string? status, string? health)
        {
            var all = (await _projectRepository.GetAllAsync()).ToList();
            var query = all.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p => 
                    p.ProjectName.ToLower().Contains(term) ||
                    p.ProjectId.ToLower().Contains(term) ||
                    p.Account.ToLower().Contains(term) ||
                    p.PM.ToLower().Contains(term) ||
                    p.TechnologyStack.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(account))
            {
                query = query.Where(p => p.Account.Equals(account, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(health))
            {
                query = query.Where(p => p.Health.Equals(health, StringComparison.OrdinalIgnoreCase));
            }

            return query.OrderBy(p => p.Account).ThenBy(p => p.ProjectName).Select(MapToDto).ToList();
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(string account, string projectId)
        {
            var all = await _projectRepository.GetAllAsync();
            var entity = all.FirstOrDefault(p => p.ProjectId.Equals(projectId, StringComparison.OrdinalIgnoreCase));
            if (entity == null) return null;

            var dto = MapToDto(entity);
            var allocations = await _allocationRepository.GetByProjectIdAsync(projectId);
            dto.Allocations = allocations.Select(a => new AllocationDto
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
                Billable = a.Billable,
                Notes = a.Notes
            }).ToList();

            return dto;
        }

        public async Task<GanttTimelineDto> GetGanttTimelineAsync()
        {
            var projects = (await _projectRepository.GetAllAsync()).Where(p => p.Status == "Active").ToList();
            var allocations = (await _allocationRepository.GetAllAsync()).ToList();
            var resources = (await _resourceRepository.GetAllAsync()).ToList();

            var ganttProjects = new List<GanttProjectItemDto>();

            foreach (var p in projects)
            {
                var projAllocations = allocations
                    .Where(a => a.ProjectId.Equals(p.ProjectId, StringComparison.OrdinalIgnoreCase))
                    .Select(a => new GanttAllocationItemDto
                    {
                        EmployeeId = a.EmployeeId,
                        EmployeeName = a.EmployeeName,
                        Role = a.Role,
                        AllocationPercent = a.AllocationPercent,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate
                    })
                    .ToList();

                ganttProjects.Add(new GanttProjectItemDto
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    Account = p.Account,
                    PM = p.PM,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Required = p.RequiredResources,
                    Allocated = p.TotalAllocated,
                    Gap = p.StaffingGap,
                    Health = p.Health,
                    ResourceAllocations = projAllocations
                });
            }

            // Overallocated resources (total allocation percent > 100)
            var overallocated = resources
                .Where(r => r.AllocationPercent > 100)
                .Select(r => new ResourceDto
                {
                    EmployeeId = r.EmployeeId,
                    Name = r.Name,
                    Designation = r.Designation,
                    AllocationPercent = r.AllocationPercent,
                    PrimarySkill = r.PrimarySkill,
                    Location = r.Location,
                    ManagerName = r.ManagerName
                })
                .ToList();

            // Underallocated / Staffing Gap projects
            var underallocated = projects
                .Where(p => p.StaffingGap > 0)
                .Select(MapToDto)
                .ToList();

            // Bench resources
            var bench = resources
                .Where(r => r.AllocationPercent == 0 || r.Availability == "Available")
                .Select(r => new ResourceDto
                {
                    EmployeeId = r.EmployeeId,
                    Name = r.Name,
                    Designation = r.Designation,
                    PrimarySkill = r.PrimarySkill,
                    ExperienceYears = r.ExperienceYears,
                    Location = r.Location,
                    ManagerName = r.ManagerName,
                    BenchStartDate = r.BenchStartDate
                })
                .ToList();

            return new GanttTimelineDto
            {
                Projects = ganttProjects,
                OverallocatedResources = overallocated,
                UnderallocatedProjects = underallocated,
                BenchResources = bench
            };
        }

        public async Task AddProjectAsync(ProjectDto dto)
        {
            var entity = MapToEntity(dto);
            await _projectRepository.AddAsync(entity);
        }

        public async Task UpdateProjectAsync(ProjectDto dto)
        {
            var entity = MapToEntity(dto);
            await _projectRepository.UpdateAsync(entity);
        }

        private static ProjectDto MapToDto(ProjectEntity entity)
        {
            return new ProjectDto
            {
                ProjectId = entity.RowKey,
                Account = entity.PartitionKey,
                ProjectName = entity.ProjectName,
                PM = entity.PM,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Status = entity.Status,
                StaffingGap = entity.StaffingGap,
                Description = entity.Description,
                TotalAllocated = entity.TotalAllocated,
                RequiredResources = entity.RequiredResources,
                Health = entity.Health,
                TechnologyStack = entity.TechnologyStack,
                Budget = entity.Budget,
                PracticeArea = entity.PracticeArea
            };
        }

        private static ProjectEntity MapToEntity(ProjectDto dto)
        {
            return new ProjectEntity
            {
                PartitionKey = dto.Account,
                RowKey = dto.ProjectId,
                ProjectName = dto.ProjectName,
                PM = dto.PM,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                StaffingGap = dto.StaffingGap,
                Description = dto.Description,
                TotalAllocated = dto.TotalAllocated,
                RequiredResources = dto.RequiredResources,
                Health = dto.Health,
                TechnologyStack = dto.TechnologyStack,
                Budget = dto.Budget,
                PracticeArea = dto.PracticeArea
            };
        }
    }
}
