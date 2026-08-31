using Microsoft.Extensions.Logging;
using MicrosoftPracticeManagement.Data.BlobStorage;
using MicrosoftPracticeManagement.Data.Entities;
using MicrosoftPracticeManagement.Data.Repositories;
using MicrosoftPracticeManagement.Services.DTOs;
using MicrosoftPracticeManagement.Services.Interfaces;

namespace MicrosoftPracticeManagement.Services.Services
{
    public class ResourceService : IResourceService
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly IAllocationRepository _allocationRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<ResourceService> _logger;

        public ResourceService(
            IResourceRepository resourceRepository,
            IAllocationRepository allocationRepository,
            ISkillRepository skillRepository,
            IBlobStorageService blobStorageService,
            ILogger<ResourceService> logger)
        {
            _resourceRepository = resourceRepository;
            _allocationRepository = allocationRepository;
            _skillRepository = skillRepository;
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        public async Task<IEnumerable<ResourceDto>> GetAllResourcesAsync()
        {
            var entities = await _resourceRepository.GetAllAsync();
            return entities.Select(MapToDto).ToList();
        }

        public async Task<(IEnumerable<ResourceDto> Items, int TotalCount)> GetFilteredResourcesAsync(
            string? search, string? account, string? project, string? technology,
            string? location, string? designation, string? status, string? availability,
            string? sortBy, bool sortDesc, int pageIndex, int pageSize)
        {
            var all = (await _resourceRepository.GetAllAsync()).ToList();

            // Filter
            var query = all.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(r => 
                    r.Name.ToLower().Contains(term) ||
                    r.EmployeeId.ToLower().Contains(term) ||
                    r.Email.ToLower().Contains(term) ||
                    r.PrimarySkill.ToLower().Contains(term) ||
                    r.SecondarySkill.ToLower().Contains(term) ||
                    r.Designation.ToLower().Contains(term) ||
                    r.Location.ToLower().Contains(term) ||
                    r.ManagerName.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(technology))
            {
                query = query.Where(r => r.PrimarySkill.Contains(technology, StringComparison.OrdinalIgnoreCase) ||
                                         r.SecondarySkill.Contains(technology, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(r => r.Location.Equals(location, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(designation))
            {
                query = query.Where(r => r.Designation.Contains(designation, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(availability))
            {
                query = query.Where(r => r.Availability.Equals(availability, StringComparison.OrdinalIgnoreCase));
            }

            // Project or Account filtering requires allocation lookup
            if (!string.IsNullOrWhiteSpace(project) || !string.IsNullOrWhiteSpace(account))
            {
                var allAllocations = (await _allocationRepository.GetAllAsync()).ToList();
                if (!string.IsNullOrWhiteSpace(project))
                {
                    var matchingEmpIds = allAllocations
                        .Where(a => a.ProjectId.Contains(project, StringComparison.OrdinalIgnoreCase) || a.ProjectName.Contains(project, StringComparison.OrdinalIgnoreCase))
                        .Select(a => a.EmployeeId)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    query = query.Where(r => matchingEmpIds.Contains(r.EmployeeId));
                }

                if (!string.IsNullOrWhiteSpace(account))
                {
                    var matchingEmpIds = allAllocations
                        .Where(a => a.Account.Contains(account, StringComparison.OrdinalIgnoreCase))
                        .Select(a => a.EmployeeId)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    query = query.Where(r => matchingEmpIds.Contains(r.EmployeeId));
                }
            }

            // Sort
            query = (sortBy?.ToLower()) switch
            {
                "name" => sortDesc ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
                "experience" => sortDesc ? query.OrderByDescending(r => r.ExperienceYears) : query.OrderBy(r => r.ExperienceYears),
                "allocation" => sortDesc ? query.OrderByDescending(r => r.AllocationPercent) : query.OrderBy(r => r.AllocationPercent),
                "location" => sortDesc ? query.OrderByDescending(r => r.Location) : query.OrderBy(r => r.Location),
                "designation" => sortDesc ? query.OrderByDescending(r => r.Designation) : query.OrderBy(r => r.Designation),
                _ => query.OrderBy(r => r.Name)
            };

            var filteredList = query.ToList();
            var totalCount = filteredList.Count;

            var paged = filteredList
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();

            return (paged, totalCount);
        }

        public async Task<ResourceDto?> GetResourceByIdAsync(string employeeId)
        {
            var all = await _resourceRepository.GetAllAsync();
            var entity = all.FirstOrDefault(r => r.EmployeeId.Equals(employeeId, StringComparison.OrdinalIgnoreCase));
            if (entity == null) return null;

            var dto = MapToDto(entity);

            // Fetch allocations & skills
            var allocations = await _allocationRepository.GetByEmployeeIdAsync(employeeId);
            dto.CurrentAllocations = allocations.Select(a => new AllocationDto
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

            var skills = await _skillRepository.GetByEmployeeIdAsync(employeeId);
            dto.Skills = skills.Select(s => new SkillDto
            {
                EmployeeId = s.EmployeeId,
                SkillName = s.SkillName,
                EmployeeName = s.EmployeeName,
                Category = s.Category,
                Level = s.Level,
                Certification = s.Certification,
                IssueDate = s.IssueDate,
                ExpiryDate = s.ExpiryDate,
                VerifiedDate = s.VerifiedDate,
                YearsOfExperience = s.YearsOfExperience,
                IsPrimary = s.IsPrimary
            }).ToList();

            return dto;
        }

        public async Task<ResourceDto?> GetResourceByManagerAndIdAsync(string managerId, string employeeId)
        {
            var entity = await _resourceRepository.GetByIdAsync(managerId, employeeId);
            return entity != null ? await GetResourceByIdAsync(employeeId) : null;
        }

        public async Task AddResourceAsync(ResourceDto dto)
        {
            var entity = MapToEntity(dto);
            await _resourceRepository.AddAsync(entity);
        }

        public async Task UpdateResourceAsync(ResourceDto dto)
        {
            var entity = MapToEntity(dto);
            await _resourceRepository.UpdateAsync(entity);
        }

        public async Task DeleteResourceAsync(string managerId, string employeeId)
        {
            await _resourceRepository.DeleteAsync(managerId, employeeId);
        }

        public async Task<Stream?> DownloadResumeAsync(string employeeId)
        {
            var resource = await GetResourceByIdAsync(employeeId);
            if (resource == null) return null;

            var fileName = $"{employeeId}_{resource.Name.Replace(" ", "_")}_Resume.pdf";
            return await _blobStorageService.DownloadFileAsync("resumes", fileName);
        }

        private static ResourceDto MapToDto(ResourceEntity entity)
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

        private static ResourceEntity MapToEntity(ResourceDto dto)
        {
            return new ResourceEntity
            {
                PartitionKey = string.IsNullOrWhiteSpace(dto.ManagerId) ? "MGR-01" : dto.ManagerId,
                RowKey = dto.EmployeeId,
                ManagerName = dto.ManagerName,
                Name = dto.Name,
                Email = dto.Email,
                Designation = dto.Designation,
                PrimarySkill = dto.PrimarySkill,
                SecondarySkill = dto.SecondarySkill,
                AllocationPercent = dto.AllocationPercent,
                Billable = dto.Billable,
                ExperienceYears = dto.ExperienceYears,
                Location = dto.Location,
                Availability = dto.Availability,
                Department = dto.Department,
                Status = dto.Status,
                HireDate = dto.HireDate,
                BenchStartDate = dto.BenchStartDate,
                ResumeBlobUrl = dto.ResumeBlobUrl,
                ProfileImageUrl = dto.ProfileImageUrl
            };
        }
    }
}
