namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class RepoVm : EntityVmBase
    {
        public required string Name { get; set; } = string.Empty;
        public required string Description { get; set; }
        public required Uri Url { get; set; }
        public int ApplicationId { get; set; }
        public ApplicationVm? ApplicationData { get; set; }

    }

}