using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IGitRepoRepository : IRepositoryEntityBase<GitRepo>
    {
        IAsyncEnumerable<GitRepo> ListRepositories();
        Task<PagedResult<GitRepo>> GetListAsync(GitRepoFilter filter);
        Task<GitRepo?> GetRepositoryDetailsAsync(int id);
        Task<bool> VerifyNameExistsAsync(string Name);
        Task<bool> VerifyUrlAlreadyExistsAsync(Uri url);
        Task<bool> VerifyDescriptionExistsAsync(string description);
        Task<bool> VerifyApplicationIdExistsAsync(int applicationId);
    }
}