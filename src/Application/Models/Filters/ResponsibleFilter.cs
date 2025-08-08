
namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class ResponsibleFilter : Filter
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public int AreaId { get; set; }
    }
}
