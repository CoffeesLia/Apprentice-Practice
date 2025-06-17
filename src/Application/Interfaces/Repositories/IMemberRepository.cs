using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IMemberRepository : IRepositoryEntityBase<Member>
    {
        Task<PagedResult<Member>> GetListAsync(MemberFilter membersFilter);
        Task<bool> IsEmailUnique(string email, int? excludeId = null);

    }
}
