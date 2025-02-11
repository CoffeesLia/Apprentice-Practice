using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class PartNumberService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<PartNumber> validator, ISupplierService supplierService)
        : EntityServiceBase<PartNumber, IPartNumberRepository>(unitOfWork, localizerFactory, validator), IPartNumberService
    {
        private readonly IStringLocalizer _partNumberLocalizer = localizerFactory.Create(typeof(PartNumberResources));
        private readonly ISupplierService _supplierService = supplierService;
        protected override IPartNumberRepository Repository => UnitOfWork.PartNumberRepository;

        /// <summary>
        /// Creates a new PartNumber.
        /// </summary>
        /// <param name="item">The PartNumber to create.</param>
        /// <returns>An OperationResult indicating the success or failure of the operation.</returns>
        public override async Task<OperationResult> CreateAsync(PartNumber item)
        {
            var result = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!result.IsValid)
                return OperationResult.InvalidData(string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
            ArgumentNullException.ThrowIfNull(item);
            item = ValidateCreateOrUpdate(item);
            if (Repository.VerifyCodeExists(item.Code!))
                return OperationResult.Conflict(_partNumberLocalizer[nameof(PartNumberResources.AlreadyExistCode)]);
            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a PartNumber by its ID.
        /// </summary>
        /// <param name="id">The ID of the PartNumber to delete.</param>
        /// <returns>An OperationResult indicating the success or failure of the operation.</returns>
        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetFullByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.NotFound(Localizer[nameof(ServiceResources.NotFound)]);
            if (item.Suppliers.Count > 0 || item.Vehicles.Count > 0)
                return OperationResult.Conflict(_partNumberLocalizer[nameof(PartNumberResources.Undeleted)]);
            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a list of PartNumbers based on the provided filter.
        /// </summary>
        /// <param name="filter">The filter to apply to the PartNumber list.</param>
        /// <returns>A PagedResult containing the filtered list of PartNumbers.</returns>
        public async Task<PagedResult<PartNumber>> GetListAysnc(PartNumberFilter filter)
        {
            filter ??= new PartNumberFilter();
            return await UnitOfWork.PartNumberRepository.GetListAsync(filter).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an existing PartNumber.
        /// </summary>
        /// <param name="item">The PartNumber to update.</param>
        /// <returns>An OperationResult indicating the success or failure of the operation.</returns>
        public override async Task<OperationResult> UpdateAsync(PartNumber item)
        {
            ArgumentNullException.ThrowIfNull(item);
            item = ValidateCreateOrUpdate(item);
            if (UnitOfWork.PartNumberRepository.VerifyCodeExists(item.Code, item.Id))
                return OperationResult.Conflict(_partNumberLocalizer[nameof(PartNumberResources.AlreadyExistCode)]);
            return await base.UpdateAsync(item).ConfigureAwait(false);
        }

        /// <summary>
        /// Validates and formats the PartNumber before creating or updating.
        /// </summary>
        /// <param name="partNumber">The PartNumber to validate and format.</param>
        /// <returns>The validated and formatted PartNumber.</returns>
        private static PartNumber ValidateCreateOrUpdate(PartNumber partNumber)
        {
            if (partNumber.Code!.Length < 11)
                partNumber.Code = partNumber.Code.PadLeft(11, '0');
            return partNumber;
        }

        /// <summary>
        /// Adds a supplier to a PartNumber.
        /// </summary>
        /// <param name="item">The PartNumberSupplier to add.</param>
        /// <returns>An OperationResult indicating the success or failure of the operation.</returns>
        public async Task<OperationResult> AddSupplierAsync(PartNumberSupplier item)
        {
            ArgumentNullException.ThrowIfNull(item);
            if (await Repository.ExistsAsync(item.PartNumberId).ConfigureAwait(false))
            {
                if (await _supplierService.ExistsAsync(item.SupplierId).ConfigureAwait(false))
                {
                    if (await Repository.ExistsSupplierAsync(item.PartNumberId, item.SupplierId).ConfigureAwait(false))
                        return OperationResult.Conflict(_partNumberLocalizer[nameof(PartNumberResources.AlreadyExistForThisSupplier)]);
                    await Repository.AddSupplierAsync(item).ConfigureAwait(false);
                    return OperationResult.Complete(Localizer[nameof(ServiceResources.RegisteredSuccessfully)]);
                }
                return OperationResult.NotFound(_partNumberLocalizer[nameof(PartNumberResources.SupplierNotFound)]);
            }
            return OperationResult.NotFound(_partNumberLocalizer[nameof(PartNumberResources.PartNumberNotFound)]);
        }

        /// <summary>
        /// Gets a supplier associated with a PartNumber.
        /// </summary>
        /// <param name="partNumberId">The ID of the PartNumber.</param>
        /// <param name="supplierId">The ID of the supplier.</param>
        /// <returns>The PartNumberSupplier if found.</returns>
        public async Task<PartNumberSupplier?> GetSupplierAsync(int partNumberId, int supplierId)
        {
            return await Repository.GetSupplierAsync(partNumberId, supplierId).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a supplier from a PartNumber.
        /// </summary>
        /// <param name="partNumberId">The ID of the PartNumber.</param>
        /// <param name="supplierId">The ID of the supplier to remove.</param>
        /// <returns>An OperationResult indicating the success or failure of the operation.</returns>
        public async Task<OperationResult> RemoveSupplierAsync(int partNumberId, int supplierId)
        {
            var item = await Repository.GetSupplierAsync(partNumberId, supplierId).ConfigureAwait(false);
            if (item == null)
                return OperationResult.NotFound(Localizer[nameof(ServiceResources.NotFound)]);

            await Repository.RemoveSupplierAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[nameof(ServiceResources.DeletedSuccessfully)]);
        }

        /// <summary>
        /// Updates a supplier associated with a PartNumber.
        /// </summary>
        /// <param name="item">The PartNumberSupplier to update.</param>
        /// <returns>An OperationResult indicating the success or failure of the operation.</returns>
        public async Task<OperationResult> UpdateSupplierAsync(PartNumberSupplier item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var itemOld = await Repository.GetSupplierAsync(item.PartNumberId, item.SupplierId).ConfigureAwait(false);
            if (itemOld == null)
                return OperationResult.NotFound(Localizer[nameof(ServiceResources.NotFound)]);
            itemOld.UnitPrice = item.UnitPrice;
            await UnitOfWork.CommitAsync().ConfigureAwait(false);
            return OperationResult.Complete(Localizer[ServiceResources.UpdatedSuccessfully]);
        }

        public async Task<PagedResult<PartNumberSupplier>> GetSupplierListAsync(PartNumberSupplierFilter filter)
        {
            return await Repository.GetSupplierListAsync(filter).ConfigureAwait(false);
        }
    }
}
