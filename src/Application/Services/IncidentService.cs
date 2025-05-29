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
    public class IncidentService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Incident> validator)
            : EntityServiceBase<Incident>(unitOfWork, localizerFactory, validator), IIncidentService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(IncidentResource));
        protected override IIncidentRepository Repository => UnitOfWork.IncidentRepository;


        public override async Task<OperationResult> CreateAsync(Incident item)
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
                    return OperationResult.Conflict(_localizer[nameof(IncidentResource.InvalidMembers)]);
                }
            }

            item.CreatedAt = DateTime.UtcNow;
            if (item.Status == default)
            {
                item.Status = IncidentStatus.Open;
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(Incident item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Validação do objeto pelo FluentValidation
            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verificar se o incidente existe
            var existingIncident = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingIncident == null)
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
                    return OperationResult.Conflict(_localizer[nameof(IncidentResource.InvalidMembers)]);
                }

                existingIncident.Members = item.Members;
            }

            // Atualiza dados básicos
            existingIncident.Title = item.Title;
            existingIncident.Description = item.Description;
            existingIncident.ApplicationId = item.ApplicationId;

            // Controle de status e datas
            if (item.Status == IncidentStatus.Closed && existingIncident.ClosedAt == null)
            {
                existingIncident.ClosedAt = DateTime.UtcNow;
            }
            else if (item.Status == IncidentStatus.Reopened)
            {
                existingIncident.ClosedAt = null;
            }

            existingIncident.Status = item.Status;

            return await base.UpdateAsync(existingIncident).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId)
        {
            return await Repository.GetMembersByApplicationIdAsync(applicationId);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var incident = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return incident != null
             ? OperationResult.Complete()
                : OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
        }

        public async Task<PagedResult<Incident>> GetListAsync(IncidentFilter incidentFilter)
        {
            incidentFilter ??= new IncidentFilter();
            return await UnitOfWork.IncidentRepository.GetListAsync(incidentFilter).ConfigureAwait(false);
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
