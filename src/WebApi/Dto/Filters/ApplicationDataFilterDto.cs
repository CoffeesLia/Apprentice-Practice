namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class ApplicationDataFilterDto : FilterDto
    {
        public int Id { get; set; }
        public int SquadId { get; set; }
        public string? Name { get; set; }
        public int AreaId { get; set; }
        public bool? External { get; set; }
        public int ResponsibleId { get; set; }
    }
}
