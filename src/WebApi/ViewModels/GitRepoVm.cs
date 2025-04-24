namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class GitRepoVm : EntityVmBase
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Uri Url { get; set; }
        public int ApplicationId { get; set; }
    }

}