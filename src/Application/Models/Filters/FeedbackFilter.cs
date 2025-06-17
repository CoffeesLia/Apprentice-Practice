using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class FeedbackFilter : Filter
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int ApplicationId { get; set; }
        public FeedbackStatus? Status { get; set; }
    }
}