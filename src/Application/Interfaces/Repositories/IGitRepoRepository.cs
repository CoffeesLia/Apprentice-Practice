using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Interfaces.Services;
using System.Linq.Expressions;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IGitRepoRepository : IRepositoryEntityBase<GitRepo>
    {
        Task<PagedResult<GitRepo>> GetListAsync(GitLabFilter filter);
        IAsyncEnumerable<GitRepo> ListRepositories();
        Task<GitRepo?> GetRepositoryDetailsAsync(int id);
        new Task<OperationResult> CreateAsync(GitRepo newRepo);
        Task<bool> VerifyAplicationsExistsAsync(int id);
        Task<bool> AnyAsync(Expression<Func<GitRepo, bool>> expression); // Adicionado
    }
}
