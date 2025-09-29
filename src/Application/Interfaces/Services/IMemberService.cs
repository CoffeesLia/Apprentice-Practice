using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IMemberService : IEntityServiceBase<Member>
    {
        Task<PagedResult<Member>> GetListAsync(MemberFilter membersFilter);
    }
}
