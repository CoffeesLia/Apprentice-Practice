// Stellantis.ProjectName.Application.Interfaces.Repositories/ISquadRepository.cs
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface ISquadRepository : IRepositoryEntityBase<Squad>
    {
        Task<PagedResult<Squad>> GetListAsync(SquadFilter squadFilter);
        Task<bool> VerifySquadExistsAsync(int id);
        Task<bool> VerifyNameAlreadyExistsAsync(string name);
        Task<Squad?> GetSquadWithApplicationsAsync(int id);
        // NOVO: Adicionado para buscar um squad pelo nome, útil na validação de atualização.
        Task<Squad?> GetByNameAsync(string name);
    }
}