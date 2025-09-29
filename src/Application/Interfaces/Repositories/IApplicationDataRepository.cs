using System.Linq.Expressions;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IApplicationDataRepository : IRepositoryEntityBase<ApplicationData>
    {
        Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter);
        Task<bool> IsApplicationNameUniqueAsync(string name, int? id = null);
        Task<ApplicationData?> GetFullByIdAsync(int id);
        Task<bool> IsResponsibleFromArea(int areaId, int responsibleId);
        Task<List<ApplicationData>> GetListAsync(Expression<Func<ApplicationData, bool>> filter);
    }
}
