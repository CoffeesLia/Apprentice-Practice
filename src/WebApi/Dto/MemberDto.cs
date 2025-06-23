namespace Stellantis.ProjectName.WebApi.Dto
{
    public class MemberDto
    {
        public required string Name { get; set; }
        public required string Role { get; set; }
        public required decimal Cost { get; set; }
        public required string Email { get; set; }
        public int SquadId { get; set; }

    }
}
