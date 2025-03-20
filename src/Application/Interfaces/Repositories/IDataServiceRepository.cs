using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IDataServiceRepository
    {
        Task<EDataService> GetServiceByIdAsync(int id);
        Task<IEnumerable<EDataService>> GetAllServicesAsync();
        Task AddServiceAsync(EDataService service);
        Task UpdateServiceAsync(EDataService service);
        Task DeleteServiceAsync(int id);
    }
}