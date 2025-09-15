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
    public class IncidentService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IStringLocalizer<NotificationResources> notificationLocalizer,
    IValidator<Incident> validator, INotificationService notificationService)
            : EntityServiceBase<Incident>(unitOfWork, localizerFactory, validator), IIncidentService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(IncidentResource));

        private readonly IStringLocalizer<NotificationResources> _notificationLocalizer = notificationLocalizer;

        private readonly INotificationService _notificationService = notificationService;
        protected override IIncidentRepository Repository => UnitOfWork.IncidentRepository;

        public override async Task<OperationResult> CreateAsync(Incident item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Força o status para "Em Aberto" ao criar
            item.Status = IncidentStatus.Open;

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

            var result = await base.CreateAsync(item).ConfigureAwait(false);

            if (result.Status == OperationResult.Complete().Status)
            {
                await _notificationService.NotifyIncidentCreatedAsync(item.Id).ConfigureAwait(false);
            }

            return result;
        }

        public override async Task<OperationResult> UpdateAsync(Incident item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Busca o incidente existente incluindo os membros
            var existingIncident = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingIncident == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            // Salva o status anterior
            var previousStatus = existingIncident.Status;

            // Salva os IDs dos membros antigos
            var oldMemberIds = existingIncident.Members?.Select(m => m.Id).ToHashSet() ?? new HashSet<int>();
            var oldMembers = existingIncident.Members?.ToList() ?? new List<Member>();

            // Atualiza propriedades simples
            existingIncident.Title = item.Title;
            existingIncident.Description = item.Description;
            existingIncident.Status = item.Status;
            existingIncident.ApplicationId = item.ApplicationId;

            // Atualiza a data de fechamento se o status for "Fechado"
            if (item.Status == IncidentStatus.Closed && existingIncident.ClosedAt == null)
            {
                existingIncident.ClosedAt = DateTime.UtcNow;
            }
            else if (item.Status != IncidentStatus.Closed)
            {
                existingIncident.ClosedAt = null;
            }

            // Atualiza membros apenas se necessário
            List<Member> newMembers = new();
            if (item.Members != null)
            {
                var memberIds = item.Members.Select(m => m.Id).ToList();
                var pagedMembers = await UnitOfWork.MemberRepository
                    .GetListAsync(m => memberIds.Contains(m.Id)).ConfigureAwait(false);
                newMembers = pagedMembers.Result.ToList();

                // Sincroniza a coleção de membros sem sobrescrever a referência
                existingIncident.Members.Clear();
                foreach (var member in newMembers)
                {
                    existingIncident.Members.Add(member);
                }
            }

            // Validação do objeto pelo FluentValidation
            var validationResult = await Validator.ValidateAsync(existingIncident).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verifica se a aplicação existe
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            // Validar se os membros estão no squad da aplicação
            if (existingIncident.Members != null && existingIncident.Members.Count > 0)
            {
                var squadId = application.SquadId;
                var invalidMemberIds = existingIncident.Members
                    .Where(m => m.SquadId != squadId)
                    .Select(m => m.Id)
                    .ToList();

                if (invalidMemberIds.Count > 0)
                {
                    return OperationResult.Conflict(_localizer[nameof(IncidentResource.InvalidMembers)]);
                }
            }

            await Repository.UpdateAsync(existingIncident, saveChanges: true).ConfigureAwait(false);

            var result = OperationResult.Complete();

            if (result.Status == OperationResult.Complete().Status && previousStatus != item.Status)
            {
                await _notificationService.NotifyIncidentStatusChangeAsync(existingIncident.Id).ConfigureAwait(false);
            }

            var newMemberIds = newMembers.Select(m => m.Id).ToHashSet();
            var addedMemberIds = newMemberIds.Except(oldMemberIds).ToList();
            if (addedMemberIds.Count > 0)
            {
                var addedMembers = newMembers.Where(m => addedMemberIds.Contains(m.Id)).ToList();
                foreach (var member in addedMembers)
                {
                    var message = _notificationLocalizer["IncidentAddMember", member.Name, existingIncident.Title];
                    await _notificationService.NotifyMembersAsync(new[] { member }, message).ConfigureAwait(false);
                }
            }

            var removedMemberIds = oldMemberIds.Except(newMemberIds).ToList();
            if (removedMemberIds.Count > 0)
            {
                var removedMembers = oldMembers.Where(m => removedMemberIds.Contains(m.Id)).ToList();
                foreach (var member in removedMembers)
                {
                    var message = _notificationLocalizer["IncidentRemoveMember", member.Name, existingIncident.Title];
                    await _notificationService.NotifyMembersAsync(new[] { member }, message).ConfigureAwait(false);
                }
            }

            return result;
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

        public async Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId)
        {
            return await Repository.GetMembersByApplicationIdAsync(applicationId).ConfigureAwait(false);
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
