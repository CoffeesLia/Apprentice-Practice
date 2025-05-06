using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface ISquadRepository : IRepositoryEntityBase<Squad>
    {
        Task<PagedResult<Squad>> GetListAsync(SquadFilter squadFilter);
        Task<bool> VerifySquadExistsAsync(int id);
        Task<bool> VerifyNameAlreadyExistsAsync(string name);
    }

}