using System.Runtime.Intrinsics.X86;
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
        // Criação da associação
        public override async Task<OperationResult> CreateAsync(Knowledge item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
                return OperationResult.InvalidData(validationResult);

            var member = await UnitOfWork.MemberRepository.GetByIdAsync(item.MemberId).ConfigureAwait(false);
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);

            if (member == null || application == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

            if (member.SquadId != application.SquadId)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.MemberApplicationMustBelongToTheSameSquad)]);

            if (await Repository.AssociationExistsAsync(item.MemberId, item.ApplicationId, member.SquadId).ConfigureAwait(false))
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.AssociationAlreadyExists)]);

            var squad = await UnitOfWork.SquadRepository.GetByIdAsync(member.SquadId).ConfigureAwait(false);

            item.SquadId = member.SquadId;
            item.Squad = squad!;
            item.Member = member;
            item.Application = application;

            await Repository.CreateAssociationAsync(item.MemberId, item.ApplicationId, item.SquadId).ConfigureAwait(false);

            var created = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            return await base.CreateAsync(created ?? item).ConfigureAwait(false);
        }

        // Atualização da associação
        public override async Task<OperationResult> UpdateAsync(Knowledge item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
                return OperationResult.InvalidData(validationResult);

            var existing = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existing == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.AssociationNotFound)]);

            if (existing.ApplicationId == item.ApplicationId)
                return OperationResult.Complete();

            var member = await UnitOfWork.MemberRepository.GetByIdAsync(existing.MemberId).ConfigureAwait(false);
            var newApplication = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);

            if (member == null || newApplication == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

            var performingUser = await UnitOfWork.MemberRepository.GetByIdAsync(item.Id).ConfigureAwait(false);
            // if (performingUser == null || !performingUser.SquadLeader)
            // return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.OnlySquadLeaderRemove)]);

            if (member.SquadId != performingUser.SquadId || newApplication.SquadId != performingUser.SquadId)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.OnlyPossibleRemoveIfBelongToTheLeadersSquad)]);

            if (member.SquadId != newApplication.SquadId)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.MemberApplicationMustBelongToTheSameSquad)]);

            if (await Repository.AssociationExistsAsync(member.Id, item.ApplicationId, member.SquadId).ConfigureAwait(false))
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.AssociationAlreadyExists)]);

            existing.ApplicationId = item.ApplicationId;

            var squad = await UnitOfWork.SquadRepository.GetByIdAsync(member.SquadId).ConfigureAwait(false);
            existing.SquadId = member.SquadId;
            existing.Squad = squad!;
            existing.Member = member;
            existing.Application = newApplication;

            return await base.UpdateAsync(existing).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var knowledge = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (knowledge == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.AssociationNotFound)]);

            return OperationResult.Complete(_localizer[nameof(KnowledgeResource.AssociationFound)]);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.AssociationNotFound)]);

            var member = await UnitOfWork.MemberRepository.GetByIdAsync(item.MemberId).ConfigureAwait(false);
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);

            if (member == null || application == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

            var performingUser = await UnitOfWork.MemberRepository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (performingUser == null || performingUser.Role != "SquadLeader")
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.OnlySquadLeaderRemove)]);

            if (member.SquadId != performingUser.SquadId || application.SquadId != performingUser.SquadId)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.OnlyPossibleRemoveIfBelongToTheLeadersSquad)]);

            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<Knowledge>> GetListAsync(KnowledgeFilter knowledgeFilter)
        {
            knowledgeFilter ??= new KnowledgeFilter();
            return await Repository.GetListAsync(knowledgeFilter).ConfigureAwait(false);
        }

        public async Task<List<ApplicationData>> ListApplicationsByMemberAsync(int memberId)
        {
            return await Repository.ListApplicationsByMemberAsync(memberId).ConfigureAwait(false);
        }

        public async Task<List<Member>> ListMembersByApplicationAsync(int applicationId)
        {
            return await Repository.ListMembersByApplicationAsync(applicationId).ConfigureAwait(false);
        }

        public async Task<bool> IsAssociationUniqueAsync(int memberId, int applicationId, int squadId)
        {
            return !await Repository.AssociationExistsAsync(memberId, applicationId, squadId).ConfigureAwait(false);
        }

        public Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Squad>> GetListAsync(SquadFilter applicationFilter)
        {
            throw new NotImplementedException();
        }
    }
}