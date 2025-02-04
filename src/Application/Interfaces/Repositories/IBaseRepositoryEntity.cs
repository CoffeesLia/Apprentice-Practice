using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using System.Linq.Expressions;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IBaseRepositoryEntity<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        Task DeleteAsync(IEnumerable<int> ids, bool saveChanges = true);
        Task DeleteAsync(int id, bool saveChanges = true);
        Task<TEntity?> GetByIdAsync(int id, bool noTracking = false);
        Task<PagedResult<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? filter = null, string? sort = null, string? sortDir = null, string? includeProperties = null, int page = 1, int pageSize = 10);
    }
}