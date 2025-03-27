using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;

namespace Stellantis.ProjectName.Application.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IStringLocalizer<ServiceResources> _localizer;


        public MemberService(IMemberRepository memberRepository, IStringLocalizer<ServiceResources> localizer)
        {
            _memberRepository = memberRepository;
            _localizer = localizer;
        }

        public async Task AddEntityMemberAsync(EntityMember entityMember)
        {
            ArgumentNullException.ThrowIfNull(entityMember, nameof(entityMember));

            if (!await Task.Run(() => _memberRepository.IsEmailUnique(entityMember.Email)).ConfigureAwait(false))
            {
                throw new InvalidOperationException(_localizer["MemberEmailAlreadyExists"]);
            }

            if (string.IsNullOrEmpty(entityMember.Name) || string.IsNullOrEmpty(entityMember.Role) || entityMember.Cost <= 0)
            {
                throw new ArgumentException(_localizer["MemberRequiredFieldsMissing"]);
            }

            await _memberRepository.AddEntityMemberAsync(entityMember).ConfigureAwait(false);
        }

        public async Task<EntityMember> GetMemberByIdAsync(Guid id)
        {
            return await _memberRepository.GetMemberByIdAsync(id).ConfigureAwait(false);
        }

        public async Task UpdateEntityMemberAsync(EntityMember entityMember)
        {
            ArgumentNullException.ThrowIfNull(entityMember, nameof(entityMember));

            if (!await Task.Run(() => _memberRepository.IsEmailUnique(entityMember.Email, entityMember.Id)).ConfigureAwait(false))
            {
                throw new InvalidOperationException(_localizer["MemberEmailAlreadyExists"]);
            }

            if (string.IsNullOrEmpty(entityMember.Name) || string.IsNullOrEmpty(entityMember.Role) || entityMember.Cost <= 0)
            {
                throw new ArgumentException(_localizer["MemberRequiredFieldsMissing"]);
            }

            await _memberRepository.UpdateEntityMemberAsync(entityMember).ConfigureAwait(false);
        }

        public async Task<IEnumerable<EntityMember>> GetMembersAsync(string? name, string? email, string? role)
        {
            return await _memberRepository.GetMembersAsync(name, email, role).ConfigureAwait(false);
        }

        public async Task DeleteMemberAsync(Guid id)
        {
            await _memberRepository.DeleteMemberAsync(id).ConfigureAwait(false);
        }

        public Task DeleteMemberByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        void IMemberService.DeleteMemberAsync(Guid memberid)
        {
            throw new NotImplementedException();
        }
    }
}
