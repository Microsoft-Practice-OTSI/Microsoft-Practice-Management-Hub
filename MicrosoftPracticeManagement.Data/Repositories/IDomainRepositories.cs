using MicrosoftPracticeManagement.Data.Entities;

namespace MicrosoftPracticeManagement.Data.Repositories
{
    public interface IProjectRepository : ITableRepository<ProjectEntity>
    {
        Task<IEnumerable<ProjectEntity>> GetByAccountAsync(string account);
        Task<IEnumerable<ProjectEntity>> GetActiveProjectsAsync();
    }

    public interface IAllocationRepository : ITableRepository<AllocationEntity>
    {
        Task<IEnumerable<AllocationEntity>> GetByProjectIdAsync(string projectId);
        Task<IEnumerable<AllocationEntity>> GetByEmployeeIdAsync(string employeeId);
    }

    public interface ISkillRepository : ITableRepository<SkillEntity>
    {
        Task<IEnumerable<SkillEntity>> GetByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<SkillEntity>> GetBySkillNameAsync(string skillName);
    }

    public interface IInnovationRepository : ITableRepository<InnovationEntity>
    {
        Task<IEnumerable<InnovationEntity>> GetByCategoryAsync(string category);
    }

    public interface ITimesheetRepository : ITableRepository<TimesheetEntity>
    {
        Task<IEnumerable<TimesheetEntity>> GetByYearMonthAsync(string yearMonth);
        Task<IEnumerable<TimesheetEntity>> GetByEmployeeIdAsync(string employeeId);
    }

    public interface ILeaveRepository : ITableRepository<LeaveEntity>
    {
        Task<IEnumerable<LeaveEntity>> GetByYearMonthAsync(string yearMonth);
        Task<IEnumerable<LeaveEntity>> GetByEmployeeIdAsync(string employeeId);
    }

    public interface IAppraisalRepository : ITableRepository<AppraisalEntity>
    {
        Task<IEnumerable<AppraisalEntity>> GetByCycleYearAsync(string cycleYear);
        Task<AppraisalEntity?> GetByEmployeeAsync(string cycleYear, string employeeId);
    }
}
