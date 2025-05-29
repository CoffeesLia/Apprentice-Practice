using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class ImprovementService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Improvement> validator)
            : EntityServiceBase<Improvement>(unitOfWork, localizerFactory, validator), IImprovementService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(ImprovementResources));
        protected override IImprovementRepository Repository => UnitOfWork.ImprovementRepository;


        public override async Task<OperationResult> CreateAsync(Improvement item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Validação do objeto pelo FluentValidation
            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verificar se a aplicação existe
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false); ;
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            // Validar se os membros estão nos squads da aplicação
            if (item.Members.Count > 0)
            {
                var validMemberIds = application.Squads
                    .SelectMany(s => s.Members)
                    .Select(m => m.Id)
                    .ToHashSet();

                var invalidMemberIds = item.Members
                    .Where(m => !validMemberIds.Contains(m.Id))
                    .Select(m => m.Id)
                    .ToList();

                if (invalidMemberIds.Count > 0)
                {
                    return OperationResult.Conflict(_localizer[nameof(ImprovementResources.InvalidMembers)]);
                }
            }

            item.CreatedAt = DateTime.UtcNow;
            if (item.StatusImprovement == default)
            {
                item.StatusImprovement = ImprovementStatus.Open;
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(Improvement item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Validação do objeto pelo FluentValidation
            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verificar se o Improvemente existe
            var existingImprovement = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingImprovement == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            // Verifica se a aplicação existe
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false); ;
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            // Valida membros se existirem
            if (item.Members.Count > 0)
            {
                var validMemberIds = application.Squads
                    .SelectMany(s => s.Members)
                    .Select(m => m.Id)
                    .ToHashSet();

                var invalidMemberIds = item.Members
                    .Where(m => !validMemberIds.Contains(m.Id))
                    .Select(m => m.Id)
                    .ToList();

                if (invalidMemberIds.Count > 0)
                {
                    return OperationResult.Conflict(_localizer[nameof(ImprovementResources.InvalidMembers)]);
                }

                existingImprovement.Members = item.Members;
            }

            // Atualiza dados básicos
            existingImprovement.Title = item.Title;
            existingImprovement.Description = item.Description;
            existingImprovement.ApplicationId = item.ApplicationId;

            // Controle de status e datas
            if (item.StatusImprovement == ImprovementStatus.Closed && existingImprovement.ClosedAt == null)
            {
                existingImprovement.ClosedAt = DateTime.UtcNow;
            }
            else if (item.StatusImprovement == ImprovementStatus.Reopened)
            {
                existingImprovement.ClosedAt = null;
            }

            existingImprovement.StatusImprovement = item.StatusImprovement;

            return await base.UpdateAsync(existingImprovement).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId)
        {
            return await Repository.GetMembersByApplicationIdAsync(applicationId);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var Improvement = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return Improvement != null
             ? OperationResult.Complete()
                : OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
        }

        public async Task<PagedResult<Improvement>> GetListAsync(ImprovementFilter ImprovementFilter)
        {
            ImprovementFilter ??= new ImprovementFilter();
            return await UnitOfWork.ImprovementRepository.GetListAsync(ImprovementFilter).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }
            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

    }
}
