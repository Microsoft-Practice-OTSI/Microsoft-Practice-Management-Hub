using MicrosoftPracticeManagement.Data.Entities;

namespace MicrosoftPracticeManagement.Data.Repositories
{
    public interface IResourceRepository : ITableRepository<ResourceEntity>
    {
        Task<IEnumerable<ResourceEntity>> GetByManagerIdAsync(string managerId);
        Task<IEnumerable<ResourceEntity>> GetAvailableResourcesAsync();
        Task<IEnumerable<ResourceEntity>> GetBillableResourcesAsync();
    }
}
