namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class IncidentVm : EntityVmBase
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public ApplicationVm? Application { get; set; }
        public ICollection<MemberVm> Members { get; set; }
    }
}