namespace Stellantis.ProjectName.WebApi.Dto
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1515:Considere tornar internos os tipos públicos", Justification = "<Pendente>")]
    public class ResponsibleDto
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string AreaId { get; set; }
        public AreaDto Area { get; set; }
    }
}
