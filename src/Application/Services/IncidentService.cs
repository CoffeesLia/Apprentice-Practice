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
    public class IncidentService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Incident> validator) 
            : EntityServiceBase<Incident>(unitOfWork, localizerFactory, validator), IIncidentService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(IncidentResource));
        protected override IIncidentRepository Repository => UnitOfWork.IncidentRepository;


        public override async Task<OperationResult> CreateAsync(Incident incident)
        {
            ArgumentNullException.ThrowIfNull(incident);

            // Validação do objeto pelo FluentValidation
            var validationResult = await Validator.ValidateAsync(incident).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verificar se a aplicação existe
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(incident.ApplicationId);
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(IncidentResource.ApplicationNotFound)]);
            }

            // Validar se os membros estão nos squads da aplicação
            if (incident.Members.Any())
            {
                var validMemberIds = application.Squads
                    .SelectMany(s => s.Members)
                    .Select(m => m.Id)
                    .ToHashSet();

                var invalidMemberIds = incident.Members
                    .Where(m => !validMemberIds.Contains(m.Id))
                    .Select(m => m.Id)
                    .ToList();

                if (invalidMemberIds.Any())
                {
                    return OperationResult.Conflict(_localizer[nameof(IncidentResource.InvalidMembers)]);
                }
            }

            incident.CreatedAt = DateTime.UtcNow;
            incident.Status = IncidentStatus.Aberto;

            return await base.CreateAsync(incident).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(Incident incident)
        {
            ArgumentNullException.ThrowIfNull(incident);

            // Validação do objeto pelo FluentValidation
            var validationResult = await Validator.ValidateAsync(incident).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verificar se o incidente existe
            var existingIncident = await Repository.GetByIdAsync(incident.Id).ConfigureAwait(false);
            if (existingIncident == null)
            {
                return OperationResult.NotFound(_localizer["IncidentNotFound"]);
            }

            // Atualizar os campos necessários
            existingIncident.Title = incident.Title;
            existingIncident.Description = incident.Description;
            existingIncident.ApplicationId = incident.ApplicationId;
            existingIncident.Status = incident.Status;
            existingIncident.ClosedAt = incident.ClosedAt;

            await Repository.UpdateAsync(existingIncident).ConfigureAwait(false);
            return OperationResult.Complete("Incident updated successfully.");
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
