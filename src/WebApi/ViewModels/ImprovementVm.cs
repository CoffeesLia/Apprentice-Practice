namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class ImprovementVm : EntityVmBase
    {
        public int Id { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public required ApplicationVm Application { get; set; }
        public IEnumerable<int> MemberIds { get; set; } = [];
    }
}
