using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class FeedbackFilterDto : FilterDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int ApplicationId { get; set; }
        public List<int> MemberId { get; set; } = [];
        public FeedbackStatus? Status { get; set; }
    }
}

