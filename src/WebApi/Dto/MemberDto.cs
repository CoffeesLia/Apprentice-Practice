namespace Stellantis.ProjectName.WebApi.Dto
{
    public class MemberDto 
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Role { get; set; }
        public decimal Cost { get; set; }
        public required string Email { get; set; }
    }
}
