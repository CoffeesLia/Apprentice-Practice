using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IPartNumberService : IBaseEntityService<PartNumber>
    {
        Task<PagedResult<PartNumber>> GetListAysnc(PartNumberFilter filter);
    }
}
