namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class GitRepoVm : EntityVmBase
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Uri? Url { get; set; }
        public int ApplicationId { get; set; }
    }

}