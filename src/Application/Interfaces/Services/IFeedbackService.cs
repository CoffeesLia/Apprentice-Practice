using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

public interface IFeedbackService : IEntityServiceBase<Feedback>
{
    Task<PagedResult<Feedback>> GetListAsync(FeedbackFilter filter);
}