using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IApplicationDataService : IEntityServiceBase<ApplicationData>
    {
        Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter);
        Task<bool> IsApplicationNameUniqueAsync(string name, int? id = null);
        Task<bool> IsResponsibleFromArea(int areaId, int responsibleId);
        Task<byte[]> ExportToCsvAsync(ApplicationFilter filter);
        Task<byte[]> ExportToPdfAsync(ApplicationFilter filter);
        Task<byte[]> ExportApplicationAsync(int id);
        Task<byte[]> ExportApplicationsAsync(ApplicationFilter filter);
    }
}
