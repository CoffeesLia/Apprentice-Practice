namespace Stellantis.ProjectName.WebApi.ViewModels
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1515:Considere tornar internos os tipos públicos", Justification = "<Pendente>")]
    public class ResponsibleVm : EntityVmBase
    {
        public required string Email { get; set; }
        public required string Nome { get; set; }
        public required string Area { get; set; }
    }
}
