using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IGitRepoService : IEntityServiceBase<GitRepo>
    {
        Task<bool> VerifyAplicationsExistsAsync(int id);
    }
}
