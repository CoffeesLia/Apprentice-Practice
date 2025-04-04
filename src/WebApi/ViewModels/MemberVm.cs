namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class MemberVm : EntityVmBase
    {
        public required string Name { get; set; }
        public required string Role { get; set; }
        public decimal Cost { get; set; }
        public required string Email { get; set; }
    }
}
