using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IRepoRepository : IRepositoryEntityBase<Repo>
    {
        Task<PagedResult<Repo>> GetListAsync(RepoFilter repofilter);
        Task<bool> IsRepoNameUniqueAsync(string Name, int applicationId, int? id = null);
        Task<bool> IsUrlUniqueAsync(Uri url, int applicationId, int? id = null));
        Task<bool> VerifyDescriptionExistsAsync(string description);
    }
}