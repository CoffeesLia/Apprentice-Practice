using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IMemberRepository
    {
        Task<bool> IsEmailUnique(string email);
        Task<EntityMember> GetMemberByIdAsync(Guid id);
        Task AddEntityMemberAsync(EntityMember entityMember);
        Task<bool>? IsEmailUnique(string email, Guid id);
        Task UpdateEntityMemberAsync(EntityMember entityMember);
        Task<IEnumerable<EntityMember>> GetMembersAsync(string? name, string? email, string? role);
        Task DeleteMemberAsync(Guid id);
    }
}
