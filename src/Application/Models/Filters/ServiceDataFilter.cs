namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class ServiceDataFilter : Filter
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Id { get; set; }
        public int ApplicationId { get; set; }
    }
}