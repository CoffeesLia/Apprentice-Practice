using System.Collections.ObjectModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.WebApi.Hubs
{
    public class NotificationService( IHubContext<NotificationHub> HubContext, Context DbContext,
    IStringLocalizer<NotificationResources> Localizer) : INotificationService
    {

        public async Task NotifyMembersAsync(IEnumerable<Member> members, string message)
        {
            ArgumentNullException.ThrowIfNull(members);

            foreach (var member in members)
            {
                DbContext.Notifications.Add(new Notification
                {
                    UserEmail = member.Email,
                    Message = message,
                    SentAt = DateTime.UtcNow
                });

                await HubContext.Clients.User(member.Email).SendAsync("ReceiveNotification", message).ConfigureAwait(false);
            }
            await DbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<Notification>> ListNotificationsAsync(string userEmail)
        {
            var result = await DbContext.Notifications
                .Where(n => n.UserEmail == userEmail)
                .ToListAsync()
                .ConfigureAwait(false);
            return result;
        }

        public async Task<IReadOnlyCollection<Notification>> ListAllNotificationsAsync()
        {
            var result = await DbContext.Notifications
                .ToListAsync()
                .ConfigureAwait(false);
            return result;
        }

        public async Task NotifyIncidentCreatedAsync(int incidentId)
        {
            var incident = await DbContext.Incidents
                .Include(i => i.Members)
                .FirstOrDefaultAsync(i => i.Id == incidentId)
                .ConfigureAwait(false);

            if (incident is null || incident.Members.Count == 0)
                return;

            foreach (var member in incident.Members)
            {
                var message = Localizer["IncidentCreatedMessage", member.Name, incident.Title];
                await NotifyMembersAsync([member], message).ConfigureAwait(false);
            }
        }

        public async Task NotifyIncidentStatusChangeAsync(int incidentId)
        {
            var incident = await DbContext.Incidents
                .Include(i => i.Members)
                .FirstOrDefaultAsync(i => i.Id == incidentId)
                .ConfigureAwait(false);

            if (incident is null || incident.Members.Count == 0)
                return;

            foreach (var member in incident.Members)
            {
                var message = Localizer["IncidentStatusChangedMessage", member.Name, incident.Title, incident.Status];
                await NotifyMembersAsync([member], message).ConfigureAwait(false);
            }
        }
    }
}