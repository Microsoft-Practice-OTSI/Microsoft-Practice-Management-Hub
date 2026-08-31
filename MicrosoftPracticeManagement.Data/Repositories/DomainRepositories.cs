using Microsoft.Extensions.Logging;
using MicrosoftPracticeManagement.Data.Entities;
using MicrosoftPracticeManagement.Data.Storage;

namespace MicrosoftPracticeManagement.Data.Repositories
{
    public class ResourceRepository : TableRepository<ResourceEntity>, IResourceRepository
    {
        public ResourceRepository(StorageContext storageContext, ILogger<ResourceRepository> logger) 
            : base("Resources", storageContext, logger)
        {
        }

        public async Task<IEnumerable<ResourceEntity>> GetByManagerIdAsync(string managerId)
        {
            var all = await GetAllAsync();
            return all.Where(r => r.ManagerId.Equals(managerId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<ResourceEntity>> GetAvailableResourcesAsync()
        {
            var all = await GetAllAsync();
            return all.Where(r => r.AllocationPercent < 100 || r.Availability == "Available");
        }

        public async Task<IEnumerable<ResourceEntity>> GetBillableResourcesAsync()
        {
            var all = await GetAllAsync();
            return all.Where(r => r.Billable);
        }
    }

    public class ProjectRepository : TableRepository<ProjectEntity>, IProjectRepository
    {
        public ProjectRepository(StorageContext storageContext, ILogger<ProjectRepository> logger) 
            : base("Projects", storageContext, logger)
        {
        }

        public async Task<IEnumerable<ProjectEntity>> GetByAccountAsync(string account)
        {
            var all = await GetAllAsync();
            return all.Where(p => p.Account.Equals(account, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<ProjectEntity>> GetActiveProjectsAsync()
        {
            var all = await GetAllAsync();
            return all.Where(p => p.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
        }
    }

    public class AllocationRepository : TableRepository<AllocationEntity>, IAllocationRepository
    {
        public AllocationRepository(StorageContext storageContext, ILogger<AllocationRepository> logger) 
            : base("Allocations", storageContext, logger)
        {
        }

        public async Task<IEnumerable<AllocationEntity>> GetByProjectIdAsync(string projectId)
        {
            var all = await GetAllAsync();
            return all.Where(a => a.ProjectId.Equals(projectId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<AllocationEntity>> GetByEmployeeIdAsync(string employeeId)
        {
            var all = await GetAllAsync();
            return all.Where(a => a.EmployeeId.Equals(employeeId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class SkillRepository : TableRepository<SkillEntity>, ISkillRepository
    {
        public SkillRepository(StorageContext storageContext, ILogger<SkillRepository> logger) 
            : base("Skills", storageContext, logger)
        {
        }

        public async Task<IEnumerable<SkillEntity>> GetByEmployeeIdAsync(string employeeId)
        {
            var all = await GetAllAsync();
            return all.Where(s => s.EmployeeId.Equals(employeeId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<SkillEntity>> GetBySkillNameAsync(string skillName)
        {
            var all = await GetAllAsync();
            return all.Where(s => s.SkillName.Equals(skillName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class InnovationRepository : TableRepository<InnovationEntity>, IInnovationRepository
    {
        public InnovationRepository(StorageContext storageContext, ILogger<InnovationRepository> logger) 
            : base("Innovations", storageContext, logger)
        {
        }

        public async Task<IEnumerable<InnovationEntity>> GetByCategoryAsync(string category)
        {
            var all = await GetAllAsync();
            return all.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class TimesheetRepository : TableRepository<TimesheetEntity>, ITimesheetRepository
    {
        public TimesheetRepository(StorageContext storageContext, ILogger<TimesheetRepository> logger) 
            : base("Timesheets", storageContext, logger)
        {
        }

        public async Task<IEnumerable<TimesheetEntity>> GetByYearMonthAsync(string yearMonth)
        {
            var all = await GetAllAsync();
            return all.Where(t => t.YearMonth.Equals(yearMonth, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<TimesheetEntity>> GetByEmployeeIdAsync(string employeeId)
        {
            var all = await GetAllAsync();
            return all.Where(t => t.EmployeeId.Equals(employeeId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class LeaveRepository : TableRepository<LeaveEntity>, ILeaveRepository
    {
        public LeaveRepository(StorageContext storageContext, ILogger<LeaveRepository> logger) 
            : base("Leaves", storageContext, logger)
        {
        }

        public async Task<IEnumerable<LeaveEntity>> GetByYearMonthAsync(string yearMonth)
        {
            var all = await GetAllAsync();
            return all.Where(l => l.YearMonth.Equals(yearMonth, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<LeaveEntity>> GetByEmployeeIdAsync(string employeeId)
        {
            var all = await GetAllAsync();
            return all.Where(l => l.EmployeeId.Equals(employeeId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class AppraisalRepository : TableRepository<AppraisalEntity>, IAppraisalRepository
    {
        public AppraisalRepository(StorageContext storageContext, ILogger<AppraisalRepository> logger) 
            : base("Appraisals", storageContext, logger)
        {
        }

        public async Task<IEnumerable<AppraisalEntity>> GetByCycleYearAsync(string cycleYear)
        {
            var all = await GetAllAsync();
            return all.Where(a => a.CycleYear.Equals(cycleYear, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<AppraisalEntity?> GetByEmployeeAsync(string cycleYear, string employeeId)
        {
            return await GetByIdAsync(cycleYear, employeeId);
        }
    }
}
