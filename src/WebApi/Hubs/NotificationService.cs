using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using System.Net.Mail;

namespace Stellantis.ProjectName.WebApi.Hubs
{
    public class NotificationService( IHubContext<NotificationHub> HubContext, Context DbContext,
    IStringLocalizer<NotificationResources> Localizer, IEmailService emailService) : INotificationService
    {
        private readonly IEmailService _emailService = emailService;
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

                try
                {
                    await _emailService.SendEmailAsync(member.Email, "Portal AMS - Nova notificação", message).ConfigureAwait(false);
                }
                catch (SmtpException smtpEx)
                {
                    Console.WriteLine($"Erro SMTP ao enviar e-mail para {member.Email}: {smtpEx.Message}");
                }
                catch (Exception)
                {
                    throw;
                }
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

        public async Task NotifyFeedbackCreatedAsync(int feedbackId)
        {
            var feedback = await DbContext.Feedbacks
                .Include(i => i.Members)
                .FirstOrDefaultAsync(i => i.Id == feedbackId)
                .ConfigureAwait(false);

            if (feedback is null || feedback.Members.Count == 0)
                return;

            foreach (var member in feedback.Members)
            {
                var message = Localizer["FeedbackCreatedMessage", member.Name, feedback.Title];
                await NotifyMembersAsync([member], message).ConfigureAwait(false);
            }
        }

        public async Task NotifyFeedbackStatusChangeAsync(int feedbackId)
        {
            var feedback = await DbContext.Feedbacks
                .Include(i => i.Members)
                .FirstOrDefaultAsync(i => i.Id == feedbackId)
                .ConfigureAwait(false);

            if (feedback is null || feedback.Members.Count == 0)
                return;

            foreach (var member in feedback.Members)
            {
                var message = Localizer["FeedbackStatusChangedMessage", member.Name, feedback.Title, feedback.Status];
                await NotifyMembersAsync([member], message).ConfigureAwait(false);
            }
        }
    }
}