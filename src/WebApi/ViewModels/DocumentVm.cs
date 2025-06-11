namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class DocumentVm : EntityVmBase
    {
        public required string Name { get; set; } = string.Empty;
        public required Uri Url { get; set; }
        public required int ApplicationId { get; set; }
        public ApplicationVm? ApplicationData { get; set; }

    }
}
