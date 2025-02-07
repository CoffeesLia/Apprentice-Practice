using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public abstract class EntityServiceBase<TEntity, TIRepository>(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory)
        : ServiceBase(unitOfWork, localizerFactory)
        where TEntity : EntityBase
        where TIRepository : IRepositoryEntityBase<TEntity>
    {
        protected abstract TIRepository Repository { get; }

        public virtual async Task<OperationResult> CreateAsync(TEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);
            await Repository.CreateAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[GeneralResources.RegisteredSuccessfully]);
        }

        public virtual async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.Error(Localizer[nameof(GeneralResources.NotFound)]);
            else
                return await DeleteAsync(item).ConfigureAwait(false);
        }

        protected async Task<OperationResult> DeleteAsync(TEntity item)
        {
            await Repository.DeleteAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[GeneralResources.DeletedSuccessfully]);
        }

        public async Task<TEntity?> GetItemAsync(int id)
        {
            return await Repository.GetByIdAsync(id).ConfigureAwait(false);
        }

        public virtual async Task<PagedResult<TEntity>> GetListAsync(Filter filter)
        {
            return await Repository.GetListAsync(sort: filter?.Sort, sortDir: filter?.SortDir, page: filter?.Page ?? 1, pageSize: filter?.RowsPerPage ?? 10).ConfigureAwait(false);
        }

        public virtual async Task<OperationResult> UpdateAsync(TEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);
            UnitOfWork.BeginTransaction();
            var itemOld = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (itemOld == null)
                return OperationResult.Error(Localizer[nameof(GeneralResources.NotFound)]);
            Repository.DetachEntity(itemOld);
            await Repository.UpdateAsync(item).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);
            return OperationResult.Complete(Localizer[GeneralResources.UpdatedSuccessfully]);
        }
    }
}
