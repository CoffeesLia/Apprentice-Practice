using System.Linq.Expressions;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IGitRepoRepository : IRepositoryEntityBase<GitRepo>
    {
        Task<PagedResult<GitRepo>> GetListAsync(GitRepoFilter filter);
        IAsyncEnumerable<GitRepo> ListRepositories();
        Task<GitRepo?> GetRepositoryDetailsAsync(int id);
        Task<OperationResult> CreateAsync(GitRepo newRepo);
        Task<bool> VerifyAplicationsExistsAsync(int id);
        Task<bool> AnyAsync(Expression<Func<GitRepo, bool>> expression); // Adicionado
        Task<bool> VerifyUrlAlreadyExistsAsync(string url);
        Task<bool> DeleteAsync(int id);
        void GetListAysnc();
        Task<bool> VerifyNameAlreadyExistsAsync(string name);
    }
}