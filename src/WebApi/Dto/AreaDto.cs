using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto
{
    public class AreaDto
    {
        public string? Name { get; set; }
        public required int ManagerId { get; set; }
    }
}