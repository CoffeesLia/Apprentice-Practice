using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly List<EntityMember> _members = new();

        public async Task<EntityMember> GetMemberByIdAsync(Guid id)
        {
            return await Task.FromResult(_members.FirstOrDefault(m => m.Id == id));
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return await Task.FromResult(!_members.Any(m => m.Email == email));
        }

        public async Task AddEntityMemberAsync(EntityMember entityMember)
        {
            _members.Add(entityMember);
            await Task.CompletedTask;
        }

        public void AddEntityMember(EntityMember entityMember)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsEmailUnique(string email)
        {
            throw new NotImplementedException();
        }
    }
}
