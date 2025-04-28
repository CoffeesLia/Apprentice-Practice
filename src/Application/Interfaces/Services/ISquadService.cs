using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface ISquadService : IEntityServiceBase<Squad>
    {
        Task<PagedResult<Squad>> GetListAsync(SquadFilter squadFilter);
    }
}
