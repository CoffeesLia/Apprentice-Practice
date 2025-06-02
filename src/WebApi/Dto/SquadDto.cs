// Stellantis.ProjectName.WebApi.Dto/SquadDto.cs
using System.Collections.Generic; // Certifique-se de que este using existe

namespace Stellantis.ProjectName.WebApi.Dto
{
    public class SquadDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        // As IDs das aplicações a serem vinculadas ao squad
        public IReadOnlyList<int> ApplicationIds { get; set; } = new List<int>();

        // As IDs dos membros a serem vinculados ao squad (se for implementar)
        public IReadOnlyList<int> MemberIds { get; set; } = new List<int>();

       
    }
}