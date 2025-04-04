using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IDataServiceRepository : IRepositoryEntityBase<DataService>
    {
        Task<DataService?> GetServiceByIdAsync(int id);
        Task<IEnumerable<DataService>> GetAllServicesAsync();
        Task AddServiceAsync(DataService service);
        Task UpdateServiceAsync(DataService service);
        Task DeleteServiceAsync(int id);
    }
}