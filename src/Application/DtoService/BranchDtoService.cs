namespace Stellantis.ProjectName.Application.DtoService
{
    public class BranchDtoService
    {
        public required string Name { get; set; }
        public required string Author { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class CreateBranchRequest
    {
        public required string NewBranchName { get; set; }
        public string SourceBranch { get; set; } = "main";
    }
}