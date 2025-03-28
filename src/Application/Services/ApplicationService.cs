using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class ApplicationService(IDataServiceRepository serviceRepository, IStringLocalizer<DataServiceResources> localizer, IValidator<DataService> validator) : IDataService
    {
        private readonly IDataServiceRepository _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
        private readonly IStringLocalizer<DataServiceResources> _localizer = localizer;
        private readonly IValidator<DataService> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        public async Task<DataService?> GetItemAsync(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id).ConfigureAwait(false);
            return service ?? throw new KeyNotFoundException(_localizer[nameof(DataServiceResources.ServiceNotFound)]);
        }

        public async Task<PagedResult<DataService>> GetListAsync(DataServiceFilter serviceFilter)
        {
            serviceFilter ??= new DataServiceFilter { Name = string.Empty };
            var result = await _serviceRepository.GetListAsync(serviceFilter).ConfigureAwait(false);
            if (result == null || !result.Result.Any())
            {
                throw new KeyNotFoundException(_localizer[nameof(DataServiceResources.ServicesNoFound)]);
            }
            return result;
        }

        public async Task<OperationResult> CreateAsync(DataService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), _localizer[nameof(DataServiceResources.ServiceCannotBeNull)]);
            }

            if (service.Name?.Length > 50 || service.Name?.Length < 3)
            {
                throw new ArgumentException(_localizer[nameof(DataServiceResources.ServiceNameLength)]);
            }

            var validationResult = await _validator.ValidateAsync(service).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }
            if (await _serviceRepository.VerifyNameAlreadyExistsAsync(service.Name ?? string.Empty).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(DataServiceResources.ServiceAlreadyExists)]);
            }
            await _serviceRepository.CreateAsync(service).ConfigureAwait(false);
            return OperationResult.Complete();
        }

        public async Task<OperationResult> UpdateAsync(DataService service)
        {
            await _serviceRepository.UpdateAsync(service).ConfigureAwait(false);
            return OperationResult.Complete();
        }

        public async Task<OperationResult> DeleteAsync(int id)
        {
            if (!await _serviceRepository.VerifyServiceExistsAsync(id).ConfigureAwait(false))
            {
                return OperationResult.NotFound(_localizer[nameof(DataServiceResources.ServiceNotFound)]);
            }
            await _serviceRepository.DeleteAsync(id).ConfigureAwait(false);
            return OperationResult.Complete();
        }

        public async Task<bool> VerifyServiceExistsAsync(int id)
        {
            if (await _serviceRepository.VerifyServiceExistsAsync(id).ConfigureAwait(false))
            {
                throw new ArgumentException(_localizer[nameof(DataServiceResources.ServiceAlreadyExists)]);
            }
            return false;
        }

        public async Task<bool> VerifyNameAlreadyExistsAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(_localizer[nameof(DataServiceResources.ServiceCannotBeNull)]);
            }

            if (await _serviceRepository.VerifyNameAlreadyExistsAsync(name).ConfigureAwait(false))
            {
                throw new ArgumentException(_localizer[nameof(DataServiceResources.ServiceAlreadyExists)]);
            }
            return false;
        }
    }
}