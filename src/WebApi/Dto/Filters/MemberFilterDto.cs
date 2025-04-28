namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class MemberFilterDto : FilterDto
    {
        public required string Name { get; set; }
        public required string Role { get; set; }
        public decimal Cost { get; set; }
        public required string Email { get; set; }
    }
}
