namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class SquadVm : EntityVmBase
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<ApplicationVm> Applications { get; set; } = new(); // Aplicações vinculadas
        public List<MemberVm> Members { get; set; } = new(); // Membros vinculados
    }
    public class ApplicationDataVm
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
