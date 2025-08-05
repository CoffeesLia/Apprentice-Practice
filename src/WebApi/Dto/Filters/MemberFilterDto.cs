namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class MemberFilterDto : FilterDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Role { get; set; }
        public decimal? Cost { get; set; }
        public string? Email { get; set; }
        public int SquadId { get; set; }
        public bool? SquadLeader { get; set; }


    }
}
