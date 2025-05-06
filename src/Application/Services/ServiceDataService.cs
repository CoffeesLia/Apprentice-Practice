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
    public class ServiceDataService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<ServiceData> validator)
        : EntityServiceBase<ServiceData>(unitOfWork, localizerFactory, validator), IServiceData
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(ServiceDataResources));
        protected override IServiceDataRepository Repository => UnitOfWork.ServiceDataRepository;

        public override async Task<OperationResult> CreateAsync(ServiceData service)
        {
            if (service == null)
            {
                return OperationResult.Conflict(_localizer[nameof(ServiceDataResources.ServiceCannotBeNull)]);
            }

            var validationResult = await Validator.ValidateAsync(service).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            var applicationData = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(service.ApplicationId).ConfigureAwait(false);
            if (applicationData == null)
            {
                return OperationResult.Conflict(_localizer[nameof(ServiceDataResources.ServiceInvalidApplicationId)]);
            }

            if (await Repository.VerifyNameExistsAsync(service.Name ?? string.Empty).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ServiceDataResources.ServiceAlreadyExists)]);
            }
            return await base.CreateAsync(service).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var service = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return service != null
               ? OperationResult.Complete()
               : OperationResult.NotFound(_localizer[nameof(ServiceDataResources.ServiceNotFound)]);
        }

        public async Task<PagedResult<ServiceData>> GetListAsync(ServiceDataFilter serviceFilter)
        {
            serviceFilter ??= new ServiceDataFilter();
            return await Repository.GetListAsync(serviceFilter).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(ServiceData service)
        {
            if (service == null)
            {
                return OperationResult.Conflict(_localizer[nameof(ServiceDataResources.ServiceCannotBeNull)]);
            }

            var validationResult = await Validator.ValidateAsync(service).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            var existingService = await Repository.GetByIdAsync(service.Id).ConfigureAwait(false);
            if (existingService == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceDataResources.ServiceNotFound)]);
            }

            var application = await Repository.GetByIdAsync(service.ApplicationId).ConfigureAwait(false);
            if (application == null)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (await Repository.VerifyNameExistsAsync(service.Name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ServiceDataResources.ServiceAlreadyExists)]);
            }
            return await base.UpdateAsync(service).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            if (!await Repository.VerifyServiceExistsAsync(id).ConfigureAwait(false))
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceDataResources.ServiceNotFound)]);
            }
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task<OperationResult> VerifyServiceExistsAsync(int id)
        {
            if (await Repository.VerifyServiceExistsAsync(id).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ServiceDataResources.ServiceAlreadyExists)]);
            }
            return OperationResult.Complete();
        }

        public async Task<OperationResult> VerifyNameExistsAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return OperationResult.Conflict(_localizer[nameof(ServiceDataResources.ServiceCannotBeNull)]);
            }

            if (await Repository.VerifyNameExistsAsync(name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ServiceDataResources.ServiceAlreadyExists)]);
            }
            return OperationResult.Complete();
        }
    }
}