using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IRepoService : IEntityServiceBase<Repo>
    {
        Task<PagedResult<Repo>> GetListAsync(RepoFilter filter);
    }
}
