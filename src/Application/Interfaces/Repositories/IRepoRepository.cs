using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IRepoRepository : IRepositoryEntityBase<Repo>
    {
        Task<PagedResult<Repo>> GetListAsync(RepoFilter repoFilter);
        Task<bool> NameAlreadyExists(string name, int applicationId, int? id = null);
        Task<bool> UrlAlreadyExists(Uri url, int applicationId, int? id = null);

    }
}