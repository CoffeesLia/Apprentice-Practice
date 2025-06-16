using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

public interface IFeedbacksService : IEntityServiceBase<Feedbacks>
{
    Task<PagedResult<Feedbacks>> GetListAsync(FeedbacksFilter filter);
}