using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Infrastructure.Data.Extensions;
using System.Linq.Expressions;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Base repository for entities.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="context">A session with the database and can be used to query and save instances of your entities.</param>
    public abstract class RepositoryBase<TEntity, TContext>(TContext context) : IRepositoryBase<TEntity> where TEntity : class where TContext : DbContext
    {
        public const char Separator = ',';

        /// <summary>
        /// Context of the database.
        /// </summary>
        protected TContext Context { get; } = context;

        /// <summary>
        /// Builds a query with the specified filter, order, and included properties.
        /// </summary>
        /// <param name="filter">An optional filter expression to apply to the query.</param>
        /// <param name="orderBy">An optional function to order the query.</param>
        /// <param name="includeProperties">A comma-separated list of related entities to include in the query results.</param>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>An <see cref="IQueryable{TEntity}"/> representing the query.</returns>
        private IQueryable<TEntity> BuildQuery(Expression<Func<TEntity, bool>>? filter, string? includeProperties)
        {
            IQueryable<TEntity> query = Context.Set<TEntity>();

            if (filter != null)
                query = query.Where(filter);

            if (!string.IsNullOrWhiteSpace(includeProperties))
                foreach (var includeProperty in includeProperties.Split([Separator], StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(includeProperty);

            return query;
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="entity">The entity to be created.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
        public async Task CreateAsync(TEntity entity, bool saveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(entity);

            await Context.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);

            if (saveChanges)
                await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a collection of new entities.
        /// </summary>
        /// <param name="entities">The entities to be created.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the entities collection is null.</exception>
        public async Task CreateAsync(IEnumerable<TEntity> entities, bool saveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(entities);

            await Context.Set<TEntity>().AddRangeAsync(entities).ConfigureAwait(false);

            if (saveChanges)
                await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an existing entity.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
        public async Task DeleteAsync(TEntity entity, bool saveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(entity);

            Context.Set<TEntity>().Remove(entity);

            if (saveChanges)
                await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a collection of existing entities.
        /// </summary>
        /// <param name="entities">The entities to be deleted.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the entities collection is null.</exception>
        public async Task DeleteAsync(IEnumerable<TEntity> entities, bool saveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(entities);

            Context.Set<TEntity>().RemoveRange(entities);

            if (saveChanges)
                await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Detaches an entity from the context.
        /// </summary>
        /// <param name="entity">The entity to be detached.</param>
        /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
        public void DetachEntity(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            Context.Entry(entity).State = EntityState.Detached;
        }

        /// <summary>
        /// Detaches a collection of entities from the context.
        /// Review: This method is not used in the project.
        /// </summary>
        /// <param name="entities">The entities to be detached.</param>
        /// <exception cref="ArgumentNullException">Thrown when the entities collection is null.</exception>
        public void DetachEntity(IEnumerable<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);
            Parallel.ForEach(entities, (entity) => Context.Entry(entity).State = EntityState.Detached);
        }

        /// <summary>
        /// Finds an entity with the given primary key values.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found, or <see langword="null" />.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the keyValues is null.</exception>
        public TEntity? Find(object[] keyValues)
        {
            ArgumentNullException.ThrowIfNull(keyValues);

            var entity = Context.Find<TEntity>(keyValues);

            // If the entity is in the Added state, it is not in the repository (Source of Truth).
            if (entity != null && Context.Entry(entity).State == EntityState.Added)
            {
                return null;
            }
            return entity;
        }

        /// <summary>
        /// Finds an entity with the given primary key values asynchronously.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>A task representing the asynchronous operation that returns the entity found, or <see langword="null" />.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the keyValues is null.</exception>
        public async ValueTask<TEntity?> FindAsync(object[] keyValues)
        {
            ArgumentNullException.ThrowIfNull(keyValues);

            var entity = await Context.FindAsync<TEntity>(keyValues).ConfigureAwait(false);

            // If the entity is in the Added state, it is not in the repository (Source of Truth).
            if (entity != null && Context.Entry(entity).State == EntityState.Added)
            {
                return null;
            }
            return entity;
        }

        /// <summary>
        /// Retrieves a paginated list of entities based on the specified filter, order, and included properties asynchronously.
        /// </summary>
        /// <param name="filter">An optional filter expression to apply to the query.</param>
        /// <param name="sort">The sort field.</param>
        /// <param name="sortDir">The sort direction (ascending or descending).</param>
        /// <param name="includeProperties">A comma-separated list of related entities to include in the query results.</param>
        /// <param name="page">The page number to retrieve. Default is 1.</param>
        /// <param name="pageSize">The number of items per page. Default is 10.</param>
        /// <returns>A task representing the asynchronous operation that returns a <see cref="PagedResult{TEntity}"/> containing the paginated list of entities and the total count.</returns>
        public async Task<PagedResult<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? filter = null, string? sort = null, string? sortDir = null, string? includeProperties = null, int page = 1, int pageSize = 10)
        {
            ValidatePaginationParameters(page, pageSize);

            IQueryable<TEntity> query = BuildQuery(filter, includeProperties);

            return await GetListAsync(query, sort, sortDir, page, pageSize).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a paginated list of entities based on the specified query asynchronously.
        /// </summary>
        /// <param name="query">The query to be executed.</param>
        /// <param name="sort">The sort field.</param>
        /// <param name="sortDir">The sort direction (ascending or descending).</param>
        /// <param name="page">The page number to retrieve. Default is 1.</param>
        /// <param name="pageSize">The number of items per page. Default is 10.</param>
        /// <returns>A task representing the asynchronous operation that returns a <see cref="PagedResult{TEntity}"/> containing the paginated list of entities and the total count.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the query is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the page or pageSize is less than 1 or when the page number exceeds the total number of pages.</exception>
        protected async Task<PagedResult<TEntity>> GetListAsync(IQueryable<TEntity> query, string? sort = null, string? sortDir = null, int page = 1, int pageSize = 10)
        {
            ArgumentNullException.ThrowIfNull(query);
            ValidatePaginationParameters(page, pageSize);

            var list = await query
                .OrderBy(sort, sortDir)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync()
                .ConfigureAwait(false);
            var total = await query.CountAsync().ConfigureAwait(false);

            if (page > 1 && total < page * pageSize)
                throw new ArgumentOutOfRangeException(nameof(page), "Page number exceeds the total number of pages.");

            return new PagedResult<TEntity>()
            {
                Result = [.. list],
                Total = total
            };
        }

        /// <summary>
        /// Saves the pending changes in the context.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SaveChangesAsync()
        {
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
        public async Task UpdateAsync(TEntity entity, bool saveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(entity);

            Context.Set<TEntity>().Update(entity);

            if (saveChanges)
                await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Updates a collection of existing entities.
        /// </summary>
        /// <param name="entities">The entities to be updated.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the entities collection is null.</exception>
        public async Task UpdateAsync(IEnumerable<TEntity> entities, bool saveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(entities);

            Context.Set<TEntity>().UpdateRange(entities);

            if (saveChanges)
                await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Validates the pagination parameters.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the page or pageSize is less than 1.</exception>
        private static void ValidatePaginationParameters(int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), "Page number must be greater than 0.");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");
        }
    }
}
