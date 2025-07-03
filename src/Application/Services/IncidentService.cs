using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Services;
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

            // Se vier apenas os IDs dos membros (ex: [{ Id = 1 }, { Id = 2 }]), busque os membros completos
            if (item.Members != null && item.Members.Count > 0)
            {
                var memberIds = item.Members.Select(m => m.Id).ToList();
                var pagedMembers = await UnitOfWork.MemberRepository
                    .GetListAsync(m => memberIds.Contains(m.Id)).ConfigureAwait(false);
                item.Members = pagedMembers.Result.ToList();
            }
            else
            {
                item.Members = new List<Member>();
            }

            // Validação do objeto pelo FluentValidation
            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verificar se a aplicação existe
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            // Validar se os membros estão no squad da aplicação
            if (item.Members != null && item.Members.Count > 0)
            {
                var squadId = application.SquadId;
                var invalidMemberIds = item.Members
                    .Where(m => m.SquadId != squadId)
                    .Select(m => m.Id)
                    .ToList();

                if (invalidMemberIds.Count > 0)
                {
                    return OperationResult.Conflict(_localizer[nameof(IncidentResource.InvalidMembers)]);
                }
            }

            item.CreatedAt = DateTime.UtcNow;
            item.Status = IncidentStatus.Open; // Sempre inicia como Open

            return await base.CreateAsync(item).ConfigureAwait(false);
        }


        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var incident = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return incident != null
             ? OperationResult.Complete()
                : OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
        }

        public override async Task<OperationResult> UpdateAsync(Incident item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Busca os membros completos a partir dos IDs
            if (item.Members != null && item.Members.Count > 0)
            {
                var memberIds = item.Members.Select(m => m.Id).ToList();
                var pagedMembers = await UnitOfWork.MemberRepository
                    .GetListAsync(m => memberIds.Contains(m.Id)).ConfigureAwait(false);
                item.Members = pagedMembers.Result.ToList();
            }

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
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            item.CreatedAt = DateTime.UtcNow;

            if (item.Status == default)
            {
                item.Status = IncidentStatus.Open;
            }

            return await base.UpdateAsync(item).ConfigureAwait(false);
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

        public async Task<PagedResult<Incident>> GetListAsync(IncidentFilter incidentFilter)
        {
            incidentFilter ??= new IncidentFilter();
            return await UnitOfWork.IncidentRepository.GetListAsync(incidentFilter).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId)
        {
            return await Repository.GetMembersByApplicationIdAsync(applicationId);
        }
    }
}
