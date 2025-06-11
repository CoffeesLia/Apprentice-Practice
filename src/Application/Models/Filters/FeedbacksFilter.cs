using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class FeedbacksFilter : Filter
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int ApplicationId { get; set; }
        public FeedbacksStatus? StatusFeedbacks { get; set; }
    }
}