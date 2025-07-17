using Microsoft.AspNetCore.SignalR;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Hubs
{
    public class ChatHub(Context context) : Hub
    {
        private readonly Context _context = context;

        public async Task SendMessage(string user, string message)
        {
            var id = Guid.NewGuid().ToString();

            var chatMessage = new Chat
            {
                Id = Guid.Parse(id),
                User = user,
                Message = message,
                SentAt = DateTime.UtcNow
            };
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            await Clients.All.SendAsync("ReceiveMessage", id, user, message).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
