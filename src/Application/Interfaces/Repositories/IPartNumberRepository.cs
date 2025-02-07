using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IPartNumberRepository : IRepositoryEntityBase<PartNumber>
    {
        bool VerifyCodeExists(string code);
        Task<PartNumber?> GetFullByIdAsync(int id);
        Task<PagedResult<PartNumber>> GetListAsync(PartNumberFilter filter);
    }
}
