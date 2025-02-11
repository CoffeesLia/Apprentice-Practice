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
    public class VehicleService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Vehicle> validator)
        : EntityServiceBase<Vehicle, IVehicleRepository>(unitOfWork, localizerFactory, validator), IVehicleService
    {
        protected override IVehicleRepository Repository => UnitOfWork.VehicleRepository;
        private readonly IStringLocalizer _vehicleLocalizer = localizerFactory.Create(typeof(VehicleResources));

        public override async Task<OperationResult> CreateAsync(Vehicle item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var (duplicate, message) = VerifyDuplicatePartNumbers(item);
            if (duplicate)
                return OperationResult.Conflict(message);
            if (await Repository.VerifyChassiExistsAsync(item.Chassi).ConfigureAwait(false))
                return OperationResult.Conflict(_vehicleLocalizer[VehicleResources.AlreadyExistChassis]);
            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetFullByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.NotFound(Localizer[nameof(ServiceResources.NotFound)]);
            if (item.PartNumbers.Count > 0)
                return OperationResult.Conflict(_vehicleLocalizer[nameof(VehicleResources.Undeleted)]);
            return await DeleteAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<Vehicle>> GetListAsync(VehicleFilter filter)
        {
            return await Repository.GetListAsync(filter).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(Vehicle item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var (duplicate, message) = VerifyDuplicatePartNumbers(item);
            if (duplicate)
                return OperationResult.Conflict(message);
            else
            {
                UnitOfWork.BeginTransaction();
                var itemOld = await Repository.GetFullByIdAsync(item.Id).ConfigureAwait(false);
                if (itemOld == null)
                    return OperationResult.NotFound(Localizer[nameof(ServiceResources.NotFound)]);
                Repository.RemovePartnumbers(item.PartNumbers);
                await Repository.UpdateAsync(item).ConfigureAwait(false);
                await UnitOfWork.CommitAsync().ConfigureAwait(false);
                return OperationResult.Complete(Localizer[ServiceResources.UpdatedSuccessfully]);
            }
        }

        private (bool, string) VerifyDuplicatePartNumbers(Vehicle item)
        {
            if (item.PartNumbers.Count > 0)
            {
                var group = item.PartNumbers.GroupBy(p => p.PartNumberId);
                var duplicates = group.Where(g => g.Count() > 1).ToList();

                if (duplicates.Count > 0)
                    return (true, _vehicleLocalizer[nameof(VehicleResources.DuplicatePartNumbers), string.Join(", ", duplicates.Select(p => p.Key))]);
            }
            return (false, string.Empty);
        }
    }
}
