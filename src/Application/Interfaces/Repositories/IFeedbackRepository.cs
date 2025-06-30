using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IFeedbackRepository : IRepositoryEntityBase<Feedback>
    {
        Task<PagedResult<Feedback>> GetListAsync(FeedbackFilter filter);

        Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId);

        // Consulta todos os incidentes vinculados a uma aplicação específica.
        Task<IEnumerable<Feedback>> GetByApplicationIdAsync(int applicationId);

        // Consulta todos os incidentes em que um membro está envolvido.
        Task<IEnumerable<Feedback>> GetByMemberIdAsync(int memberId);

        // Consulta todos os incidentes com um determinado status.
        Task<IEnumerable<Feedback>> GetByStatusAsync(FeedbackStatus status);
    }
}