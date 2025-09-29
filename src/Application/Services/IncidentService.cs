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

            item.Status = IncidentStatus.Open;

            if (item.Members != null && item.Members.Count > 0)
            {
                var memberIds = item.Members.Select(m => m.Id).ToList();
                var pagedMembers = await UnitOfWork.MemberRepository
                    .GetListAsync(m => memberIds.Contains(m.Id)).ConfigureAwait(false);
                item.Members = [.. pagedMembers.Result];
            }
            else
            {
                item.Members = [];
            }

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

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

            var existingIncident = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingIncident == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            var previousStatus = existingIncident.Status;

            var oldMemberIds = existingIncident.Members?.Select(m => m.Id).ToHashSet() ?? [];
            var oldMembers = existingIncident.Members?.ToList() ?? [];

            existingIncident.Title = item.Title;
            existingIncident.Description = item.Description;
            existingIncident.Status = item.Status;
            existingIncident.ApplicationId = item.ApplicationId;

            if (item.Status == IncidentStatus.Closed && existingIncident.ClosedAt == null)
            {
                existingIncident.ClosedAt = DateTime.UtcNow;
            }
            else if (item.Status != IncidentStatus.Closed)
            {
                existingIncident.ClosedAt = null;
            }

            List<Member> newMembers = [];
            if (item.Members != null)
            {
                var memberIds = item.Members.Select(m => m.Id).ToList();
                var pagedMembers = await UnitOfWork.MemberRepository
                    .GetListAsync(m => memberIds.Contains(m.Id)).ConfigureAwait(false);
                newMembers = [.. pagedMembers.Result];

                existingIncident.Members ??= [];

                existingIncident.Members.Clear();
                foreach (var member in newMembers)
                {
                    existingIncident.Members.Add(member);
                }
            }

            var validationResult = await Validator.ValidateAsync(existingIncident).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

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
                    IEnumerable<Member> members = [member];
                    await _notificationService.NotifyMembersAsync(members, message).ConfigureAwait(false);
                }
            }

            var removedMemberIds = oldMemberIds.Except(newMemberIds).ToList();
            if (removedMemberIds.Count > 0)
            {
                var removedMembers = oldMembers.Where(m => removedMemberIds.Contains(m.Id)).ToList();
                foreach (var member in removedMembers)
                {
                    var message = _notificationLocalizer["IncidentRemoveMember", member.Name, existingIncident.Title];
                    await _notificationService.NotifyMembersAsync([member], message).ConfigureAwait(false);
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

        public async Task<PagedResult<Incident>> GetListAsync(IncidentFilter filter)
        {
            filter ??= new IncidentFilter();
            return await UnitOfWork.IncidentRepository.GetListAsync(filter).ConfigureAwait(false);
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
