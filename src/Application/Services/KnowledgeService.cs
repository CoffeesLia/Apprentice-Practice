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

        // métodos herdados de EntityServiceBase

        // valida os dados; varifica se já existe associação; busca membro e aplicação; garante que os dois pertencem ao mesmo squad
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

            // membro e aplicação devem pertencer ao mesmo squad
            if (member.SquadId != application.SquadId)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.MemberApplicationMustBelongToTheSameSquad)]);

            // evitar duplicidade
            if (await Repository.AssociationExistsAsync(item.MemberId, item.ApplicationId).ConfigureAwait(false))
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.AssociationAlreadyExists)]);

            // registra o squad no momento da associação
            item.AssociatedSquadId = member.SquadId;

            await Repository.CreateAssociationAsync(item.MemberId, item.ApplicationId, member.SquadId).ConfigureAwait(false);
            return await base.CreateAsync(item).ConfigureAwait(false);

        }

        // valida se o parâmeto não é nulo; busca associação existente pelo ID; caso não encontre, retorna erro; se a aplicação não mudou, retorna sucesso; se membro ou aplicação não existir, retorna mensagem; valida se a aplicação pertence ao mesmo squad do membro;
        public override async Task<OperationResult> UpdateAsync(Knowledge item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
                return OperationResult.InvalidData(validationResult);

            var existing = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existing == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.AssociationNotFound)]);

            // se não mudou a aplicação, não precisa atualizar
            if (existing.ApplicationId == item.ApplicationId)
                return OperationResult.Complete();

            var member = await UnitOfWork.MemberRepository.GetByIdAsync(existing.MemberId).ConfigureAwait(false);
            var newApplication = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);

            if (member == null || newApplication == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

            // apenas líder do squad pode editar
            var performingUser = await UnitOfWork.MemberRepository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (performingUser == null || !performingUser.SquadLeader)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.OnlySquadLeaderRemove)]);

            // só permite se membro e aplicação ainda pertencem ao squad do líder
            if (member.SquadId != performingUser.SquadId || newApplication.SquadId != performingUser.SquadId)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.OnlyPossibleRemoveIfBelongToTheLeadersSquad)]);

            // associações passadas não podem ser editadas
            if (existing.AssociatedSquadId != performingUser.SquadId)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.CannotEditOrRemovePastAssociation)]);

            if (member.SquadId != newApplication.SquadId)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.MemberApplicationMustBelongToTheSameSquad)]);

            if (await Repository.AssociationExistsAsync(member.Id, item.ApplicationId).ConfigureAwait(false))
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.AssociationAlreadyExists)]);

            existing.ApplicationId = item.ApplicationId;
            existing.AssociatedSquadId = member.SquadId;
            return await base.UpdateAsync(item).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var knowledge = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (knowledge == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.AssociationNotFound)]);

            // retorna os dados relevantas, caso necesário
            return OperationResult.Complete(_localizer[nameof(KnowledgeResource.AssociationFound)]);
        }

        // remove uma associação pelo ID; busca a associação, caso não exista, rertorna erro; se existir, remove 
        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.AssociationNotFound)]);

            var member = await UnitOfWork.MemberRepository.GetByIdAsync(item.MemberId).ConfigureAwait(false);
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);

            if (member == null || application == null)
                return OperationResult.NotFound(_localizer[nameof(KnowledgeResource.MemberApplicationNotFound)]);

            // apenas líder do squad pode remover
            var performingUser = await UnitOfWork.MemberRepository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (performingUser == null || performingUser.Role != "SquadLeader")
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.OnlySquadLeaderRemove)]);

            // só permite se membro e aplicação ainda pertencem ao squad do líder
            if (member.SquadId != performingUser.SquadId || application.SquadId != performingUser.SquadId)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.OnlyPossibleRemoveIfBelongToTheLeadersSquad)]);

            // associações passadas não podem ser removidas
            if (item.AssociatedSquadId != performingUser.SquadId)
                return OperationResult.Conflict(_localizer[nameof(KnowledgeResource.CannotEditOrRemovePastAssociation)]);

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

        public async Task<bool> IsAssociationUniqueAsync(int memberId, int applicationId)
        {
            return !await Repository.AssociationExistsAsync(memberId, applicationId).ConfigureAwait(false);
        }

        public Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter)
        {
            throw new NotImplementedException();
        }
    }
}
