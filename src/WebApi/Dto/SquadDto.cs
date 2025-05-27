using System;

namespace Stellantis.ProjectName.WebApi.Dto
{
    public class SquadDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IReadOnlyList<int> ApplicationIds { get; set; } = new List<int>(); // IDs das aplicações
        public IReadOnlyList<int> MemberIds { get; set; } = new List<int>(); // IDs dos membros

        public void AddApplicationId(int applicationId)
        {
            _applicationIds.Add(applicationId);
        }

        public void RemoveApplicationId(int applicationId)
        {
            _applicationIds.Remove(applicationId);
        }
    }
}
