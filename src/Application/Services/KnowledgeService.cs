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
    public class KnowledgeService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Knowledge> validator)
        : EntityServiceBase<Knowledge>(unitOfWork, localizerFactory, validator), IKnowledgeService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(KnowledgeResource));

        protected override IKnowledgeRepository Repository =>
            UnitOfWork.KnowledgeRepository;

        // Métodos herdados de EntityServiceBase
        public override async Task<OperationResult> CreateAsync(Knowledge item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (await Repository.AssociationExistsAsync(item.MemberId, item.ApplicationId).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.AssociationAlreadyExists)]);
            }

            var member = await UnitOfWork.MemberRepository.GetByIdAsync(item.MemberId).ConfigureAwait(false);
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);

            if (member == null || application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);
            }

            if (member.SquadId != application.SquadId)
            {
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.MemberApplicationMustBelongToTheSameSquad)]);
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.AssociationNotFound)]);

            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(Knowledge item)
        {
            var validationResult = new FluentValidation.Results.ValidationResult(
                new[] { new FluentValidation.Results.ValidationFailure("", _localizer[nameof(KnowledgeResource.UnsupportedMembershipUpdate)]) }
            );
            return OperationResult.InvalidData(validationResult);
        }


        public async Task<Knowledge?> GetItemAsync(int id)
        {
            return await Repository.GetByIdAsync(id).ConfigureAwait(false);
        }

        // Métodos customizados da interface IKnowledgeService

        public async Task CreateAssociationAsync(int memberId, int applicationId, int currentSquadId)
        {
            var member = await UnitOfWork.MemberRepository.GetByIdAsync(memberId).ConfigureAwait(false);
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(applicationId).ConfigureAwait(false);

            if (member == null || application == null)
                throw new InvalidOperationException(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

            if (member.SquadId != currentSquadId || application.SquadId != currentSquadId)
                throw new InvalidOperationException(_localizer[nameof(KnowledgeResource.MemberApplicationMustBelongToTheSameSquad)]);

            if (await Repository.AssociationExistsAsync(memberId, applicationId).ConfigureAwait(false))
                throw new InvalidOperationException(_localizer[nameof(KnowledgeResource.AssociationAlreadyExists)]);

            await Repository.CreateAssociationAsync(memberId, applicationId).ConfigureAwait(false);
        }

        public async Task RemoveAssociationAsync(int memberId, int applicationId, int leaderSquadId)
        {
            var member = await UnitOfWork.MemberRepository.GetByIdAsync(memberId).ConfigureAwait(false);
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(applicationId).ConfigureAwait(false);

            if (member == null || application == null)
                throw new InvalidOperationException(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

            if (member.SquadId != leaderSquadId || application.SquadId != leaderSquadId)
                throw new InvalidOperationException(_localizer[nameof(KnowledgeResource.OnlyPossibleRemoveIfBelongToTheLeadersSquad)]);

            await Repository.RemoveAssociationAsync(memberId, applicationId).ConfigureAwait(false);
        }

        public async Task<List<ApplicationData>> ListApplicationsByMemberAsync(int memberId)
        {
            return await Repository.ListApplicationsByMemberAsync(memberId).ConfigureAwait(false);
        }

        public async Task<List<Member>> ListMembersByApplicationAsync(int applicationId)
        {
            return await Repository.ListMembersByApplicationAsync(applicationId).ConfigureAwait(false);
        }

        public Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Knowledge>> GetListAsync(KnowledgeFilter knowledgeFilter)
        {
            throw new NotImplementedException();
        }
    }
}
