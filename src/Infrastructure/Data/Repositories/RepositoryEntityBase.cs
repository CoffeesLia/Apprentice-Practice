using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using System.Linq.Expressions;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Base repository for entities based on <see cref="EntityBase"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity based on BaseEntity.</typeparam>
    /// <param name="context">A session with the database and can be used to query and save instances of your entities.</param>
    public abstract class RepositoryEntityBase<TEntity, TContext>(TContext context) : RepositoryBase<TEntity, TContext>(context) where TEntity : EntityBase where TContext : DbContext
    {
        /// <summary>
        /// Deletes an entity by its id.
        /// </summary>
        /// <param name="id">Entity's id.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            var entity = await GetByIdAsync(id).ConfigureAwait(false);
            Context.Set<TEntity>().Remove(entity!);

            if (saveChanges)
                await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a list of entities by their ids.
        /// </summary>
        /// <param name="ids">List of entities' ids.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteAsync(IEnumerable<int> ids, bool saveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var entities = await Context.Set<TEntity>().Where(p => ids.Contains(p.Id)).ToListAsync().ConfigureAwait(false);
            Context.Set<TEntity>().RemoveRange(entities);

            if (saveChanges)
                await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an entity by id with optional no-tracking and include properties.
        /// </summary>
        /// <param name="id">Entity's id.</param>
        /// <param name="noTracking">Indicates whether to track changes in the context.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the single element.</returns>
        public async Task<TEntity?> GetByIdAsync(int id, bool noTracking = false)
        {
            return await GetByIdWithIncludeAsync(id, noTracking).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an entity by id with include properties.
        /// </summary>
        /// <param name="id">Entity's id.</param>
        /// <param name="includeProperties">Specifies related entities to include in the query results.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the single element.</returns>
        public async Task<TEntity?> GetByIdWithIncludeAsync(int id, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return await GetByIdWithIncludeAsync(id, false, includeProperties).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an entity by id with include properties and optional no-tracking.
        /// </summary>
        /// <param name="id">Entity's id.</param>
        /// <param name="noTracking">Indicates whether to track changes in the context.</param>
        /// <param name="includeProperties">Specifies related entities to include in the query results.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the single element.</returns>
        public async Task<TEntity?> GetByIdWithIncludeAsync(int id, bool noTracking, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = Context.Set<TEntity>();

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                    query = query.Include(includeProperty);
            }

            if (noTracking)
                query = query.AsNoTracking();
            return await query.SingleOrDefaultAsync(p => p.Id == id).ConfigureAwait(false);
        }
    }
}
