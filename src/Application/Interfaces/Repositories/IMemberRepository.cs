using Stellantis.ProjectName.Application.Interfaces.Repositories;
using System.Collections.Generic;
using System;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Linq.Expressions;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IMemberRepository : IRepositoryEntityBase<Member>
    {
        Task<PagedResult<Member>> GetListAsync(MemberFilter membersFilter);
        Task<bool> IsEmailUnique(string email, int? excludeId = null);
        Task<bool> AnyAsync(Expression<Func<Member, bool>> predicate);

    }
}
