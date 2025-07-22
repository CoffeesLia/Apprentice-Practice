using Stellantis.ProjectName.Application.Interfaces.Services;
using System;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IMemberService : IEntityServiceBase<Member>
    {
        Task<PagedResult<Member>> GetListAsync(MemberFilter membersFilter);
    }
}
