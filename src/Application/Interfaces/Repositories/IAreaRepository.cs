using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IAreaRepository : IRepositoryEntityBase<Area>
    {
        public Task<Area?> GetByNameAsync(string name);
    }
}
