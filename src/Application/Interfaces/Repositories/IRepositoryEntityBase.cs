using System.Linq.Expressions;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IRepositoryEntityBase<TEntity> : IRepositoryBase<TEntity> where TEntity : BaseEntity
    {
        Task DeleteAsync(int id, bool saveChanges = true);
        Task<TEntity?> GetByIdAsync(int id);
        Task<PagedResult<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? filter = null, string? sort = null, string? sortDir = null, string? includeProperties = null, int page = 1, int pageSize = 10);
    }
}