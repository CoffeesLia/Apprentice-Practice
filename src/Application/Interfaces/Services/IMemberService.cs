using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IMemberService
    {
        Task AddEntityMemberAsync(EntityMember entityMember);
        Task<EntityMember> GetMemberByIdAsync(Guid id);
    }
}
