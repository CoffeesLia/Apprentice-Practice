using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using FluentValidation;



namespace Stellantis.ProjectName.Application.Services
{
    public class MemberService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Member> validator)
        : EntityServiceBase<Member>(unitOfWork, localizerFactory, validator), IMemberService
    {
        private new IStringLocalizer Localizer => localizerFactory.Create(typeof(MemberResource));

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

            if (!await Repository.IsEmailUnique(item.Email).ConfigureAwait(false))
            {
                return OperationResult.Conflict(Localizer[nameof(MemberResource.MemberEmailAlreadyExists)]);
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            return await Repository.GetByIdAsync(id).ConfigureAwait(false) is Member member
                ? OperationResult.Complete()
                : OperationResult.NotFound(Localizer[nameof(ServiceResources.NotFound)]);
        }

        public override async Task<OperationResult> UpdateAsync(Member item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (!await Repository.IsEmailUnique(item.Email).ConfigureAwait(false))
            {
                return OperationResult.Conflict(Localizer[nameof(MemberResource.MemberEmailAlreadyExists)]);
            }

            return await base.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<Member>> GetListAsync(MemberFilter memberFilter)
        {
            memberFilter ??= new MemberFilter();
            return await UnitOfWork.MemberRepository.GetListAsync(memberFilter).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.NotFound(base.Localizer[nameof(MemberResource.MemberNotFound)]);
            return await base.DeleteAsync(item).ConfigureAwait(false);
        }
    }
}