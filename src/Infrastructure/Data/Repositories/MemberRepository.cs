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

        public async Task UpdateEntityMemberAsync(EntityMember entityMember)
        {
            var existingMember = _members.FirstOrDefault(m => m.Id == entityMember.Id);
            if (existingMember == null)
            {
                throw new KeyNotFoundException("MemberNotFound");
            }

            existingMember.Name = entityMember.Name;
            existingMember.Role = entityMember.Role;
            existingMember.Cost = entityMember.Cost;
            existingMember.Email = entityMember.Email;

            await Task.CompletedTask;   
        }

        public async Task<IEnumerable<EntityMember>> GetMembersAsync(string? name, string? email, string? role)
        {
            var query = _members.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(m => m.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(m => m.Email.Contains(email, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(m => m.Role.Contains(role, StringComparison.OrdinalIgnoreCase));
            }

            return await Task.FromResult(query.ToList());
        }

        public async Task DeleteMemberAsync(Guid id)
        {
            var member = _members.FirstOrDefault(m => m.Id == id);
            if (member == null)
            {
                throw new KeyNotFoundException("MemberNotFound");
            }

            _members.Remove(member);
            await Task.CompletedTask;
        }

        public Task<bool>? IsEmailUnique(string email, Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
