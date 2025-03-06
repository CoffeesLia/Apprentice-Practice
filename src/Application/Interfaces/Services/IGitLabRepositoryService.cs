using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Models;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IGitLabRepositoryService : IEntityServiceBase<EntityGitLabRep>
    {
        Task<PagedResult<EntityGitLabRep>> GetListAsync(GitLabFilter filter);
        IAsyncEnumerable<EntityGitLabRep> ListRepositories();
        Task<EntityGitLabRep?> GetRepositoryDetailsAsync(int id);
        new Task<OperationResult> CreateAsync(EntityGitLabRep newRepo);
    }
}
