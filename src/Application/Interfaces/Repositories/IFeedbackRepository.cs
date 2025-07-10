using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IFeedbackRepository : IRepositoryEntityBase<Feedback>
    {
        Task<PagedResult<Feedback>> GetListAsync(FeedbackFilter filter);

        Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId);

    }
}