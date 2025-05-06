using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IGitRepoService : IEntityServiceBase<GitRepo>
    {
        Task<PagedResult<GitRepo>> GetListAsync(GitRepoFilter gitRepoFilter);
        Task<bool> VerifyAplicationsExistsAsync(int id);
    }
}
