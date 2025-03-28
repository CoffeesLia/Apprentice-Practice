namespace Stellantis.ProjectName.WebApi.ViewModels
{
    internal class GitRepoVm
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Url { get; set; }
        public int ApplicationId { get; set; }
    }

}