using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IResponsibleService : IEntityServiceBase<Responsible>
    {
        Task<PagedResult<Responsible>> GetListAsync(ResponsibleFilter responsibleFilter);
       

    }
}
