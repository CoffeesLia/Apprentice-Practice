using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Infrastructure.Data.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly CleanArchBaseContext _context;
        readonly char[] arrSeparator = [','];

        protected BaseRepository(CleanArchBaseContext context)
        {
            _context = context;
        }

        public TEntity? Find(object[] keyValues)
        {
            return _context.Find<TEntity>(keyValues);
        }

        public ValueTask<TEntity?> FindAsync(object[] keyValues)
        {
            return _context.FindAsync<TEntity>(keyValues);
        }

        public PaginationDTO<TEntity> GetList(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, string? includeProperties = null, int page = 1, int pageSize = 10)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();


            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(arrSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var list = query.Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            return new PaginationDTO<TEntity>()
            {
                Result = list,
                Total = query.Count()
            };
        }

        public async Task<PaginationDTO<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? filter = null, string? sort = null, string? sortDir = null, string? includeProperties = null, int page = 1, int pageSize = 10)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(arrSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (!string.IsNullOrEmpty(sort))
            {
                query = query.OrderBy($"{sort} {sortDir}");
            }

            var list = await query.Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

            return new PaginationDTO<TEntity>()
            {
                Result = list,
                Total = query.Count()
            };
        }

        public async Task<PaginationDTO<TEntity>> GetListQueryableAsync(IQueryable<TEntity> query, string? sort = null, string? sortDir = null, int page = 1, int pageSize = 10)
        {

            if (!string.IsNullOrEmpty(sort))
            {
                query = query.OrderBy($"{sort} {sortDir}");
            }

            var list = await query.Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

            return new PaginationDTO<TEntity>()
            {
                Result = list,
                Total = query.Count()
            };
        }



        public async Task<TEntity?> GetById(int id)
        {
            return await _context.Set<TEntity>().SingleOrDefaultAsync(p => p.Id == id);
        }

        public async Task<TEntity?> GetByIdAsNoTracking(int id)
        {
            return await _context.Set<TEntity>().AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
        }

        public async Task<TEntity> GetByIdWithInclude(int id, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.Where(p => p.Id == id).SingleAsync();
        }

        public async Task<TEntity> GetByIdWithIncludeAsNoTracking(int id, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.Where(p => p.Id == id).AsNoTracking().SingleAsync();
        }

        public async Task Create(TEntity entity, bool saveChanges = true)
        {
            await _context.Set<TEntity>().AddAsync(entity);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task Create(IEnumerable<TEntity> entitys, bool saveChanges = true)
        {
            foreach (var entity in entitys)
            {
                await _context.Set<TEntity>().AddAsync(entity);
            }

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task Update(TEntity entity, bool saveChanges = true)
        {
            _context.Set<TEntity>().Update(entity);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task Update(IEnumerable<TEntity> entitys, bool saveChanges = true)
        {
            foreach (var entity in entitys)
            {
                _context.Set<TEntity>().Update(entity);
            }

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public void DetachEntity(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }

        public void DetachEntity(IEnumerable<TEntity> entitys)
        {
            foreach (var entity in entitys)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
        }

        public async Task Delete(int id, bool saveChanges = true)
        {
            var entity = await GetById(id);
            _context.Set<TEntity>().Remove(entity!);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task Delete(IEnumerable<int> ids, bool saveChanges = true)
        {
            foreach (var id in ids)
            {
                var entity = await GetById(id);
                _context.Set<TEntity>().Remove(entity!);
            }

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task Delete(TEntity entity, bool saveChanges = true)
        {
            _context.Set<TEntity>().Remove(entity);

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task Delete(IEnumerable<TEntity> entitys, bool saveChanges = true)
        {
            foreach (var entity in entitys)
            {
                _context.Set<TEntity>().Remove(entity);
            }

            if (saveChanges)
                await _context.SaveChangesAsync();
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
