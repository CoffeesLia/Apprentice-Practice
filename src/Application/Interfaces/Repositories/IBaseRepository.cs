namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    /// <summary>
    /// Generic interface for base repositories.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IBaseRepository<in TEntity> where TEntity : class
    {
        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="entity">The entity to be created.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateAsync(TEntity entity, bool saveChanges = true);

        /// <summary>
        /// Creates a collection of new entities.
        /// </summary>
        /// <param name="entities">The entities to be created.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateAsync(IEnumerable<TEntity> entities, bool saveChanges = true);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(TEntity entity, bool saveChanges = true);

        /// <summary>
        /// Updates a collection of existing entities.
        /// </summary>
        /// <param name="entities">The entities to be updated.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(IEnumerable<TEntity> entities, bool saveChanges = true);

        /// <summary>
        /// Deletes an existing entity.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(TEntity entity, bool saveChanges = true);

        /// <summary>
        /// Deletes a collection of existing entities.
        /// </summary>
        /// <param name="entities">The entities to be deleted.</param>
        /// <param name="saveChanges">Indicates whether changes should be saved immediately.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(IEnumerable<TEntity> entities, bool saveChanges = true);

        /// <summary>
        /// Detaches an entity from the context.
        /// </summary>
        /// <param name="entity">The entity to be detached.</param>
        void DetachEntity(TEntity entity);

        /// <summary>
        /// Detaches a collection of entities from the context.
        /// </summary>
        /// <param name="entities">The entities to be detached.</param>
        void DetachEntity(IEnumerable<TEntity> entities);

        /// <summary>
        /// Saves the pending changes in the context.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveChangesAsync();
    }
}