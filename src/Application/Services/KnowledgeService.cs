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
        protected override IKnowledgeRepository Repository => UnitOfWork.KnowledgeRepository;

        public override async Task<OperationResult> CreateAsync(Knowledge item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
                return OperationResult.InvalidData(validationResult);

            var member = await UnitOfWork.MemberRepository.GetByIdAsync(item.MemberId).ConfigureAwait(false);
            if (member == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

            foreach (var appId in item.ApplicationIds)
            {
                var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(appId).ConfigureAwait(false);

                if (application == null)
                    return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

                if (item.Status == KnowledgeStatus.Atual && member.SquadId != application.SquadId)
                    return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.MemberApplicationMustBelongToTheSameSquad)]);

                if (await Repository.AssociationExistsAsync(item.MemberId, appId, member.SquadId, item.Status).ConfigureAwait(false))
                    return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.AssociationAlreadyExists)]);
            }

            item.SquadId = member.SquadId;
            var squad = await UnitOfWork.SquadRepository.GetByIdAsync(member.SquadId).ConfigureAwait(false);
            if (squad == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);
            item.Squad = squad;
            item.Member = member;

            item.Applications.Clear();
            foreach (var appId in item.ApplicationIds)
            {
                var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(appId).ConfigureAwait(false);
                if (application != null)
                    item.Applications.Add(application);
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(Knowledge item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var existing = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existing == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.AssociationNotFound)]);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
                return OperationResult.InvalidData(validationResult);

            if (existing.Status == KnowledgeStatus.Passado)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.CannotEditOrRemovePastAssociation)]);

            var member = await UnitOfWork.MemberRepository.GetByIdAsync(existing.MemberId).ConfigureAwait(false);
            if (member == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

            foreach (var appId in item.ApplicationIds)
            {
                var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(appId).ConfigureAwait(false);
                if (application == null)
                    return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

                if (member.SquadId != application.SquadId)
                    return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.MemberApplicationMustBelongToTheSameSquad)]);

                if (await Repository.AssociationExistsAsync(member.Id, appId, member.SquadId, item.Status).ConfigureAwait(false))
                    return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.AssociationAlreadyExists)]);
            }

            existing.Status = item.Status;
            existing.SquadId = member.SquadId;
            var squad = await UnitOfWork.SquadRepository.GetByIdAsync(member.SquadId).ConfigureAwait(false);
            if (squad == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);
            existing.Squad = squad;
            existing.Member = member;

            existing.ApplicationIds.Clear();
            foreach (var id in item.ApplicationIds)
                existing.ApplicationIds.Add(id);

            existing.Applications.Clear();
            foreach (var appId in item.ApplicationIds)
            {
                var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(appId).ConfigureAwait(false);
                if (application != null)
                    existing.Applications.Add(application);
            }

            return await base.UpdateAsync(existing).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.AssociationNotFound)]);

            if (item.Status == KnowledgeStatus.Passado)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.CannotEditOrRemovePastAssociation)]);

            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var knowledge = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return knowledge != null
                ? OperationResult.Complete()
                : OperationResult.NotFound(_localizer[nameof(KnowledgeResource.AssociationNotFound)]);
        }

        public async Task<PagedResult<Knowledge>> GetListAsync(KnowledgeFilter knowledgeFilter)
        {

            knowledgeFilter ??= new KnowledgeFilter();
            return await Repository.GetListAsync(knowledgeFilter).ConfigureAwait(false);
        }
        public async Task<Squad?> GetSquadByMemberAsync(int memberId)
        {
            var member = await UnitOfWork.MemberRepository.GetByIdAsync(memberId).ConfigureAwait(false);
            if (member == null)
                return null;
            return await UnitOfWork.SquadRepository.GetByIdAsync(member.SquadId).ConfigureAwait(false);
        }

        public async Task<ICollection<ApplicationData>> GetApplicationsByMemberAsync(int memberId)
        {
            return await Repository.ListApplicationsByMemberAsync(memberId, KnowledgeStatus.Atual).ConfigureAwait(false);
        }

        public async Task<List<Squad>> GetSquadsByApplicationAsync(int applicationId)
        {
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(applicationId).ConfigureAwait(false);
            if (application?.SquadId != null)
            {
                var squad = await UnitOfWork.SquadRepository.GetByIdAsync(application.SquadId.Value).ConfigureAwait(false);
                return squad != null ? [squad] : [];
            }
            return [];
        }

        public async Task<List<ApplicationData>> GetApplicationsBySquadAsync(int squadId)
        {
            var filter = new ApplicationFilter { SquadId = squadId };
            var pagedResult = await UnitOfWork.ApplicationDataRepository.GetListAsync(filter).ConfigureAwait(false);
            return [.. pagedResult.Result];
        }

        public async Task<List<OperationResult>> CreateMultipleAsync(int memberId, int[] applicationIds, KnowledgeStatus status)
        {
            ArgumentNullException.ThrowIfNull(applicationIds);

            var results = new List<OperationResult>();
            var knowledge = new Knowledge
            {
                MemberId = memberId,
                Status = status
            };
            foreach (var id in applicationIds)
                knowledge.ApplicationIds.Add(id);

            var result = await CreateAsync(knowledge).ConfigureAwait(false);
            results.Add(result);
            return results;
        }
    }
}