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
    public class SupplierService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Supplier> validator)
        : EntityServiceBase<Supplier, ISupplierRepository>(unitOfWork, localizerFactory, validator), ISupplierService
    {
        private readonly IStringLocalizer _supplierLocalizer = localizerFactory.Create(typeof(SupplierResources));
        protected override ISupplierRepository Repository => UnitOfWork.SupplierRepository;

        public override async Task<OperationResult> CreateAsync(Supplier item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var (duplicate, message) = VerifyDuplicatePartNumbers(item);
            if (duplicate)
                return OperationResult.Conflict(message);
            if (await Repository.VerifyCodeExistsAsync(item.Code).ConfigureAwait(false))
                return OperationResult.Conflict(_supplierLocalizer[nameof(SupplierResources.AlreadyExistCode)]);
            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetFullByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.NotFound(Localizer[nameof(GeneralResources.NotFound)]);
            if (item.PartNumbers!.Count > 0)
                return OperationResult.Conflict(_supplierLocalizer[nameof(SupplierResources.Undeleted)]);
            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(Supplier item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var (duplicate, message) = VerifyDuplicatePartNumbers(item);
            if (duplicate)
                return OperationResult.Conflict(message);
            return await base.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<Supplier>> GetListAsync(SupplierFilter filter)
        {
            return await UnitOfWork.SupplierRepository.GetListAsync(filter).ConfigureAwait(false);
        }

        private (bool, string) VerifyDuplicatePartNumbers(Supplier item)
        {
            if (item.PartNumbers.Count > 0)
            {
                var group = item.PartNumbers.GroupBy(p => p.PartNumberId);
                var duplicates = group.Where(g => g.Count() > 1).ToList();

                if (duplicates.Count > 0)
                    return (true, _supplierLocalizer[nameof(SupplierResources.DuplicatePartNumbers), string.Join(", ", duplicates.Select(p => p.Key))]);
            }
            return (false, string.Empty);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await Repository.ExistsAsync(id).ConfigureAwait(false);
        }
    }
}
