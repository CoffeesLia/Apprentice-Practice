namespace Stellantis.ProjectName.WebApi.Dto
{
    public class DocumentDto
    {
        public string Name { get; set; } = string.Empty;
        public required Uri Url { get; set; }
        public int ApplicationId { get; set; }
    }

}
