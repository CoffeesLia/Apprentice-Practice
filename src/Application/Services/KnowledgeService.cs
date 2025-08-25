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
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);

            if (member == null || application == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

            if (member.SquadId != application.SquadId)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.MemberApplicationMustBelongToTheSameSquad)]);

            // Verifica se já existe associação com o mesmo status
            if (await Repository.AssociationExistsAsync(item.MemberId, item.ApplicationId, member.SquadId, item.Status).ConfigureAwait(false))
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.AssociationAlreadyExists)]);

            item.SquadId = member.SquadId;
            item.Squad = await UnitOfWork.SquadRepository.GetByIdAsync(member.SquadId).ConfigureAwait(false)!;
            item.Member = member;
            item.Application = application;

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
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(existing.ApplicationId).ConfigureAwait(false);

            if (member == null || application == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

            if (member.SquadId != application.SquadId)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.MemberApplicationMustBelongToTheSameSquad)]);

            // Verifica se já existe associação com o mesmo status
            if (await Repository.AssociationExistsAsync(member.Id, item.ApplicationId, member.SquadId, item.Status).ConfigureAwait(false))
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.AssociationAlreadyExists)]);

            existing.ApplicationId = item.ApplicationId;
            existing.Status = item.Status;
            existing.SquadId = member.SquadId;
            existing.Squad = await UnitOfWork.SquadRepository.GetByIdAsync(member.SquadId).ConfigureAwait(false)!;
            existing.Member = member;
            existing.Application = application;

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
    }
}