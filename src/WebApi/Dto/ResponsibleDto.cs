namespace Stellantis.ProjectName.WebApi.Dto
{
    public class ResponsibleDto
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required int AreaId { get; set; }
        public required AreaDto Area { get; set; }
    }
}
