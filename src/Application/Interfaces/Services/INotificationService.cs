using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task NotifyIncidentCreatedAsync(int incidentId);
        Task NotifyIncidentStatusChangeAsync(int incidentId);
        Task NotifyFeedbackCreatedAsync(int feedbackId);
        Task NotifyFeedbackStatusChangeAsync(int feedbackId);
        Task NotifyMembersAsync(IEnumerable<Member> members, string message);
    }
}
