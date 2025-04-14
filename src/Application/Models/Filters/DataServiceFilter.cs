namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class DataServiceFilter : Filter
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int ServiceId { get; set; }
    }
}