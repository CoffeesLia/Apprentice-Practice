namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class DataServiceFilter : Filter
    {
        public required string Name { get; set; }
        public int ServiceId { get; set; }
    }
}