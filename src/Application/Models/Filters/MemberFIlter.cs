namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class MemberFilter : Filter
    {
        public string? Name { get; set; }
        public string? Role { get; set; }
        public decimal? Cost { get; set; }
        public int Id { get; set; }
        public string? Email { get; set; }
    }
}
