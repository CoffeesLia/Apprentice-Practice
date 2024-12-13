using Domain.DTO;
using System.Linq.Expressions;

namespace Domain.Interfaces
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task Create(TEntity entity, bool saveChanges = true);
        Task Create(IEnumerable<TEntity> entitys, bool saveChanges = true);

        Task Update(TEntity entity, bool saveChanges = true);
        Task Update(IEnumerable<TEntity> entitys, bool saveChanges = true);

        Task Delete(int id, bool saveChanges = true);
        Task Delete(IEnumerable<int> ids, bool saveChanges = true);

        Task Delete(TEntity entity, bool saveChanges = true);
        Task Delete(IEnumerable<TEntity> entitys, bool saveChanges = true);

        void DetachEntity(TEntity entity);
        void DetachEntity(IEnumerable<TEntity> entitys);

        Task SaveChanges();


        Task<TEntity?> GetById(int id);
        Task<TEntity?> GetByIdAsNoTracking(int id);
        Task<TEntity> GetByIdWithInclude(int id, params Expression<Func<TEntity, object>>[] includeProperties);
        Task<TEntity> GetByIdWithIncludeAsNoTracking(int id, params Expression<Func<TEntity, object>>[] includeProperties);
        Task<PaginationDTO<TEntity>> GetListQueryableAsync(IQueryable<TEntity> query, string? sort = null, string? sortDir = null, int page = 1, int pageSize = 10);
    }
}
