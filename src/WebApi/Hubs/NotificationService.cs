using System.Collections.ObjectModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Stellantis.ProjectName.WebApi.Hubs
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly Context _dbContext;

        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            Context dbContext
        )
        {
            _hubContext = hubContext;
            _dbContext = dbContext;
        }

        public async Task NotifyMembersAsync(IEnumerable<Member> members, string message)
        {
            ArgumentNullException.ThrowIfNull(members);

            foreach (var member in members)
            {
                _dbContext.Notifications.Add(new Notification
                {
                    UserEmail = member.Email,
                    Message = message,
                    SentAt = DateTime.UtcNow
                });

                await _hubContext.Clients.User(member.Email).SendAsync("ReceiveNotification", message).ConfigureAwait(false);
            }
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<Notification>> ListNotificationsAsync(string userEmail)
        {
            var result = await _dbContext.Notifications
                .Where(n => n.UserEmail == userEmail)
                .ToListAsync()
                .ConfigureAwait(false);
            return result;
        }

        public async Task<IReadOnlyCollection<Notification>> ListAllNotificationsAsync()
        {
            var result = await _dbContext.Notifications
                .ToListAsync()
                .ConfigureAwait(false);
            return result;
        }

        public async Task NotifyIncidentCreatedAsync(int incidentId)
        {
            var incident = await _dbContext.Incidents
                .Include(i => i.Members)
                .FirstOrDefaultAsync(i => i.Id == incidentId)
                .ConfigureAwait(false);

            if (incident is null || incident.Members.Count == 0)
                return;

            foreach (var member in incident.Members)
            {
                var message = $"Olá {member.Name}, um novo incidente associado a você foi criado: {incident.Title}";
                await NotifyMembersAsync(new[] { member }, message).ConfigureAwait(false);
            }
        }

        public async Task NotifyIncidentStatusChangeAsync(int incidentId)
        {
            var incident = await _dbContext.Incidents
                .Include(i => i.Members)
                .FirstOrDefaultAsync(i => i.Id == incidentId)
                .ConfigureAwait(false);

            if (incident is null || incident.Members.Count == 0)
                return;

            foreach (var member in incident.Members)
            {
                var message = $"Olá {member.Name}, o status do incidente associado foi alterado para: {incident.Status}";
                await NotifyMembersAsync(new[] { member }, message).ConfigureAwait(false);
            }
        }
    }

    public class NotificationResult
    {
        public bool IsNotFound { get; }
        public ReadOnlyCollection<string> Emails { get; }

        private NotificationResult(bool isNotFound, ReadOnlyCollection<string>? emails = null)
        {
            IsNotFound = isNotFound;
            Emails = emails ?? new ReadOnlyCollection<string>([]);
        }

        public static NotificationResult NotFound() => new(true);
        public static NotificationResult Success(ReadOnlyCollection<string> emails) => new(false, emails);
    }
}