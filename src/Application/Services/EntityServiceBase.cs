using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    /// <summary>
    /// Base class for entity services providing common CRUD operations.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TIRepository">The type of the repository.</typeparam>
    public abstract class EntityServiceBase<TEntity>(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<TEntity> validator)
        : ServiceBase(unitOfWork, localizerFactory)
        where TEntity : EntityBase
    {
        /// <summary>
        /// Gets the repository for the entity.
        /// </summary>
        protected abstract IRepositoryEntityBase<TEntity> Repository { get; }

        /// <summary>
        /// Gets the validator for the entity.
        /// </summary>
        protected IValidator<TEntity> Validator { get; } = validator;

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="item">The entity to create.</param>
        /// <returns>An OperationResult indicating the success or failure of the operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the item is null.</exception>
        public virtual async Task<OperationResult> CreateAsync(TEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);
            await Repository.CreateAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[ServiceResources.RegisteredSuccessfully]);
        }

        /// <summary>
        /// Deletes an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to delete.</param>
        /// <returns>An OperationResult indicating the success or failure of the operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the item is null.</exception>
        public virtual async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.NotFound(Localizer[nameof(ServiceResources.NotFound)]);
            return await DeleteAsync(item).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="item">The entity to delete.</param>
        /// <returns>An OperationResult indicating the success or failure of the operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the item is null.</exception>
        protected async Task<OperationResult> DeleteAsync(TEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);
            await Repository.DeleteAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[ServiceResources.DeletedSuccessfully]);
        }

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve.</param>
        /// <returns>The entity if found, otherwise null.</returns>
        public async Task<TEntity?> GetItemAsync(int id)
        {
            return await Repository.GetByIdAsync(id).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a paginated list of entities based on the provided filter.
        /// </summary>
        /// <param name="filter">The filter to apply to the entity list.</param>
        /// <returns>A PagedResult containing the filtered list of entities.</returns>
        public virtual async Task<PagedResult<TEntity>> GetListAsync(Filter filter)
        {
            return await Repository.GetListAsync(sort: filter?.Sort, sortDir: filter?.SortDir, page: filter?.Page ?? 1, pageSize: filter?.PageSize ?? 10).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="item">The entity to update.</param>
        /// <returns>An OperationResult indicating the success or failure of the operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the item is null.</exception>
        public virtual async Task<OperationResult> UpdateAsync(TEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var itemOld = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (itemOld == null)
                return OperationResult.NotFound(Localizer[nameof(ServiceResources.NotFound)]);
            Repository.DetachEntity(itemOld);
            await Repository.UpdateAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[ServiceResources.UpdatedSuccessfully]);
        }
    }
}
