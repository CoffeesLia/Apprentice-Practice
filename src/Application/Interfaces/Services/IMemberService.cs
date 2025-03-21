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
        Task UpdateEntityMemberAsync(EntityMember entityMember);
        Task<IEnumerable<EntityMember>> GetMembersAsync(string? name, string? email, string? role);
        Task DeleteMemberByIdAsync(Guid id);
        void DeleteMemberAsync(Guid memberid);
    }
}
