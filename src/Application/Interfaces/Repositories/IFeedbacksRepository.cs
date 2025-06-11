using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IFeedbacksRepository : IRepositoryEntityBase<Feedbacks>
    {
        Task<PagedResult<Feedbacks>> GetListAsync(FeedbacksFilter filter);

        Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId);

        Task<IEnumerable<Feedbacks>> GetByApplicationIdAsync(int applicationId);

        Task<IEnumerable<Feedbacks>> GetByMemberIdAsync(int memberId);

        Task<IEnumerable<Feedbacks>> GetByStatusAsync(FeedbacksStatus statusFeedbacks);
    }
}