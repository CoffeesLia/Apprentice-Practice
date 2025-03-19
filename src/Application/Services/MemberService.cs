using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Resources;
using Microsoft.Extensions.Localization;

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
            if (!await Task.Run(() => _memberRepository.IsEmailUnique(entityMember.Email)))
            {
                throw new InvalidOperationException(_localizer["MemberEmailAlreadyExists"]);
            }

            if (string.IsNullOrEmpty(entityMember.Name) || string.IsNullOrEmpty(entityMember.Role) || entityMember.Cost <= 0)
            {
                throw new ArgumentException(_localizer["MemberRequiredFieldsMissing"]);
            }

            await _memberRepository.AddEntityMemberAsync(entityMember);
        }

        public Task<EntityMember> GetMemberByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
