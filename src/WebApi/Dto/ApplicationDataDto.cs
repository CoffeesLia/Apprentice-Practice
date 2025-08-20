namespace Stellantis.ProjectName.WebApi.Dto
{
    public class ApplicationDataDto
    {
        public required string Name { get; set; }
        public int? SquadId { get; set; }
        public int AreaId { get; set; }
        public int ResponsibleId { get; set; }
        public string? Description { get; set; }
        public bool? External { get; set; }
    }
}
