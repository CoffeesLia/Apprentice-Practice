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

            foreach (var appId in item.ApplicationIds)
            {
                var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(appId).ConfigureAwait(false);

                if (item.Status == KnowledgeStatus.Atual && member.SquadId != application.SquadId)
                    return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.MemberApplicationMustBelongToTheSameSquad)]);

                if (await Repository.AssociationExistsAsync(item.MemberId, appId, member.SquadId, item.Status).ConfigureAwait(false))
                    return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.AssociationAlreadyExists)]);
            }

            item.SquadId = member.SquadId;
            item.Squad = await UnitOfWork.SquadRepository.GetByIdAsync(member.SquadId).ConfigureAwait(false)!;
            item.Member = member;

            // Preenche a coleção Applications
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

            // Não permite editar associação passada
            if (existing.Status == KnowledgeStatus.Passado)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.CannotEditOrRemovePastAssociation)]);

            var member = await UnitOfWork.MemberRepository.GetByIdAsync(existing.MemberId).ConfigureAwait(false);

            // Validação para todas as aplicações
            foreach (var appId in item.ApplicationIds)
            {
                var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(appId).ConfigureAwait(false);
                if (member == null || application == null)
                    return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

                if (member.SquadId != application.SquadId)
                    return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.MemberApplicationMustBelongToTheSameSquad)]);

                if (await Repository.AssociationExistsAsync(member.Id, appId, member.SquadId, item.Status).ConfigureAwait(false))
                    return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.AssociationAlreadyExists)]);
            }

            existing.Status = item.Status;
            existing.SquadId = member.SquadId;
            existing.Squad = await UnitOfWork.SquadRepository.GetByIdAsync(member.SquadId).ConfigureAwait(false)!;
            existing.Member = member;

            // Atualiza ApplicationIds
            existing.ApplicationIds.Clear();
            foreach (var id in item.ApplicationIds)
                existing.ApplicationIds.Add(id);

            // Atualiza Applications
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

            // Não permite remover associação passada
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

        public async Task<PagedResult<Knowledge>> GetListAsync(KnowledgeFilter filter)
        {
            filter ??= new KnowledgeFilter();
            return await Repository.GetListAsync(filter).ConfigureAwait(false);
        }

        // buscar squad do membro
        public async Task<Squad?> GetSquadByMemberAsync(int memberId)
        {
            var member = await UnitOfWork.MemberRepository.GetByIdAsync(memberId);
            if (member == null)
                return null;
            return await UnitOfWork.SquadRepository.GetByIdAsync(member.SquadId);
        }

        // buscar aplicações do membro 
        public async Task<ICollection<ApplicationData>> GetApplicationsByMemberAsync(int memberId)
        {
            return await Repository.ListApplicationsByMemberAsync(memberId, KnowledgeStatus.Atual);
        }

        // buscar squads da aplicação
        public async Task<List<Squad>> GetSquadsByApplicationAsync(int applicationId)
        {
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(applicationId);
            if (application?.SquadId != null)
            {
                var squad = await UnitOfWork.SquadRepository.GetByIdAsync(application.SquadId.Value);
                return squad != null ? new List<Squad> { squad } : new List<Squad>();
            }
            return new List<Squad>();
        }

        // buscar aplicações do squad
        public async Task<List<ApplicationData>> GetApplicationsBySquadAsync(int squadId)
        {
            var filter = new ApplicationFilter { SquadId = squadId };
            var pagedResult = await UnitOfWork.ApplicationDataRepository.GetListAsync(filter);
            return pagedResult.Result.ToList();
        }

        public async Task<List<OperationResult>> CreateMultipleAsync(int memberId, int[] applicationIds, KnowledgeStatus status)
        {
            var results = new List<OperationResult>();
            var knowledge = new Knowledge
            {
                MemberId = memberId,
                Status = status
            };
            foreach (var id in applicationIds)
                knowledge.ApplicationIds.Add(id);

            var result = await CreateAsync(knowledge);
            results.Add(result);
            return results;
        }
    }
}