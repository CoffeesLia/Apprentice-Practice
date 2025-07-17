using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.WebApi.Hubs; // Alterado para usar NotificationService diretamente

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController(NotificationService notificationService, IIncidentService incidentService) : ControllerBase
    {
        [HttpGet("incident/{incidentId}")]
        public async Task<IActionResult> NotifyIncidentStatusChange(int incidentId)
        {
            var incident = await incidentService.GetItemAsync(incidentId).ConfigureAwait(false);
            if (incident is null || incident.Members.Count == 0)
                return NotFound();

            var message = $"O status do seu incidente foi alterado para: {incident.Status}";
            await notificationService.NotifyMembersAsync(incident.Members, message).ConfigureAwait(false);

            return Ok(new { Notified = incident.Members.Select(m => m.Email).ToList() });
        }

        // Outros endpoints para listar notificações, etc.
        [HttpGet("{email}")]
        public async Task<IActionResult> List(string email)
        {
            var notifications = await notificationService.ListNotificationsAsync(email).ConfigureAwait(false);
            return Ok(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notifications = await notificationService.ListAllNotificationsAsync().ConfigureAwait(false);
            return Ok(notifications);
        }
    }
}