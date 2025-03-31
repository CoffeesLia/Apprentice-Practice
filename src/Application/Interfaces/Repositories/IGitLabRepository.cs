using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Filters;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IGitLabRepository : IEntityServiceBase<EntityGitLabRepository>
    {
        Task<PagedResult<EntityGitLabRepository>> GetListAsync(GitLabFilter filter);
        IAsyncEnumerable<EntityGitLabRepository> ListRepositories();
        Task<EntityGitLabRepository?> GetRepositoryDetailsAsync(int id);
        new Task<OperationResult> CreateAsync(EntityGitLabRepository newRepo);
    }
}
