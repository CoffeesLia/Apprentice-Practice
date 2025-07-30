using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class MemberService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Member> validator)
        : EntityServiceBase<Member>(unitOfWork, localizerFactory, validator), IMemberService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(MemberResource));

        protected override IMemberRepository Repository =>
            UnitOfWork.MemberRepository;

        public override async Task<OperationResult> CreateAsync(Member item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            var squad = await UnitOfWork.SquadRepository.GetByIdAsync(item.SquadId).ConfigureAwait(false);
            if (squad == null)
            {
                return OperationResult.Conflict(_localizer[nameof(ServiceDataResources.ServiceInvalidApplicationId)]);
            }

            if (!await Repository.IsEmailUnique(item.Email).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(MemberResource.MemberEmailAlreadyExists)]);
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var member = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return member != null
                ? OperationResult.Complete()
                : OperationResult.NotFound(Localizer[nameof(ServiceResources.NotFound)]);
        }

        public override async Task<OperationResult> UpdateAsync(Member item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (item.Role == "SquadLeader")
            {
                bool alreadyExists = await Repository.AnyAsync(
                    m => m.SquadId == item.SquadId && m.Role == "SquadLeader" && m.Id != item.Id).ConfigureAwait(false);

                if (alreadyExists)
                    return OperationResult.Conflict(_localizer[nameof(MemberResource.LeaderSquadAlreadyExists)]);
            }

            var existingMember = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingMember == null)
            {
                return OperationResult.NotFound(_localizer[nameof(MemberResource.MemberNotFound)]);
            }

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (!await Repository.IsEmailUnique(item.Email, item.Id).ConfigureAwait(false))
            {
                return OperationResult.Conflict(Localizer[nameof(MemberResource.MemberEmailAlreadyExists)]);
            }

            item.Email = existingMember.Email;

            return await base.UpdateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
            {
                return OperationResult.NotFound(base.Localizer[nameof(MemberResource.MemberNotFound)]);
            }

            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<Member>> GetListAsync(MemberFilter membersFilter)
        {
            membersFilter ??= new MemberFilter();
            return await Repository.GetListAsync(membersFilter).ConfigureAwait(false);
        }
    }
}