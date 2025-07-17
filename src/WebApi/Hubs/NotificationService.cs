using Microsoft.AspNetCore.SignalR;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Stellantis.ProjectName.WebApi.Hubs
{
    public class NotificationService(
        IHubContext<NotificationHub> hubContext,
        Context dbContext
    )
    {
        public async Task NotifyMembersAsync(IEnumerable<Member> members, string message)
        {
            ArgumentNullException.ThrowIfNull(members);

            foreach (var member in members)
            {
                dbContext.Notifications.Add(new Notification
                {
                    UserEmail = member.Email,
                    Message = message,
                    SentAt = DateTime.UtcNow
                });

                await hubContext.Clients.User(member.Email).SendAsync("ReceiveNotification", message).ConfigureAwait(false);
            }
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<Notification>> ListNotificationsAsync(string userEmail)
        {
            var result = await dbContext.Notifications
                .Where(n => n.UserEmail == userEmail)
                .ToListAsync()
                .ConfigureAwait(false);
            return result;
        }

        public async Task<IReadOnlyCollection<Notification>> ListAllNotificationsAsync()
        {
            var result = await dbContext.Notifications
                .ToListAsync()
                .ConfigureAwait(false);
            return result;
        }
    }
}