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
    public class ManagerService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Manager> validator)
        : EntityServiceBase<Manager>(unitOfWork, localizerFactory, validator), IManagerService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(ManagerResources));
        protected override IManagerRepository Repository => UnitOfWork.ManagerRepository;

        public override async Task<OperationResult> CreateAsync(Manager manager)
        {
            if (manager == null)
            {
                return OperationResult.Conflict(_localizer[nameof(ManagerResources.ManagerCannotBeNull)]);
            }

            var validationResult = await Validator.ValidateAsync(manager).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (await Repository.VerifyNameExistsAsync(manager.Name ?? string.Empty).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ManagerResources.ManagerAlreadyExists)]);
            }
            return await base.CreateAsync(manager).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var service = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return service != null
               ? OperationResult.Complete()
               : OperationResult.NotFound(_localizer[nameof(ManagerResources.ManagerNotFound)]);
        }

        public override async Task<OperationResult> UpdateAsync(Manager manager)
        {
            if (manager == null)
            {
                return OperationResult.Conflict(_localizer[nameof(ManagerResources.ManagerCannotBeNull)]);
            }

            var existingManager = await Repository.GetByIdAsync(manager.Id).ConfigureAwait(false);
            if (existingManager == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ManagerResources.ManagerNotFound)]);
            }

            if (await Repository.VerifyNameExistsAsync(manager.Name).ConfigureAwait(false))
            {
                var managerWithSameName = await Repository.GetListAsync(new ManagerFilter { Name = manager.Name }).ConfigureAwait(false);

                if (managerWithSameName.Result.Any(s => s.Id != manager.Id))
                {
                    return OperationResult.Conflict(_localizer[nameof(ManagerResources.ManagerAlreadyExists)]);
                }
            }

            var validationResult = await Validator.ValidateAsync(manager).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            return await base.UpdateAsync(manager).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            if (!await Repository.VerifyManagerExistsAsync(id).ConfigureAwait(false))
            {
                return OperationResult.NotFound(_localizer[nameof(ManagerResources.ManagerNotFound)]);
            }

            var areas = await UnitOfWork.AreaRepository.GetListAsync(new AreaFilter { ManagerId = id }).ConfigureAwait(false);
            if (areas.Result.Any())
            {
                return OperationResult.Conflict(_localizer[nameof(ManagerResources.ManagerLinkedArea)]);
            }

            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<Manager>> GetListAsync(ManagerFilter managerFilter)
        {
            managerFilter ??= new ManagerFilter();
            return await Repository.GetListAsync(managerFilter).ConfigureAwait(false);
        }

        public async Task<OperationResult> VerifyManagerExistsAsync(int id)
        {
            if (await Repository.VerifyManagerExistsAsync(id).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ManagerResources.ManagerAlreadyExists)]);
            }
            return OperationResult.Complete();
        }

        public async Task<OperationResult> VerifyNameExistsAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return OperationResult.Conflict(_localizer[nameof(ManagerResources.ManagerCannotBeNull)]);
            }

            if (await Repository.VerifyNameExistsAsync(name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ManagerResources.ManagerAlreadyExists)]);
            }
            return OperationResult.Complete();
        }
    }
}