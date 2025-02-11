using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using System.Linq.Expressions;

namespace Stellantis.ProjectName.Infrastructure.Data
{
    public class AreaRepository : IAreaRepository
    {
        public Task CreateAsync(Area entity, bool saveChanges = true)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(IEnumerable<Area> entities, bool saveChanges = true)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id, bool saveChanges = true)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Area entity, bool saveChanges = true)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(IEnumerable<Area> entities, bool saveChanges = true)
        {
            throw new NotImplementedException();
        }

        public void DetachEntity(Area entity)
        {
            throw new NotImplementedException();
        }

        public void DetachEntity(IEnumerable<Area> entities)
        {
            throw new NotImplementedException();
        }

        public Task<Area?> GetByIdAsync(int id, bool noTracking = false)
        {
            throw new NotImplementedException();
        }

        public Task<Area?> GetByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Area>> GetListAsync(Expression<Func<Area, bool>>? filter = null, string? sort = null, string? sortDir = null, string? includeProperties = null, int page = 1, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Area entity, bool saveChanges = true)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(IEnumerable<Area> entities, bool saveChanges = true)
        {
            throw new NotImplementedException();
        }
    }
}