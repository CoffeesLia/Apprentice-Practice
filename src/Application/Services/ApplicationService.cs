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
    public class ApplicationService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<DataService> validator)
        : EntityServiceBase<DataService>(unitOfWork, localizerFactory, validator), IDataService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(DataServiceResources));
        protected override IDataServiceRepository Repository => UnitOfWork.DataServiceRepository;

        public override async Task<OperationResult> CreateAsync(DataService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), _localizer[nameof(DataServiceResources.ServiceCannotBeNull)]);
            }

            var validationResult = await Validator.ValidateAsync(service).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }
            if (await Repository.VerifyNameAlreadyExistsAsync(service.Name ?? string.Empty).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(DataServiceResources.ServiceAlreadyExists)]);
            }
            await Repository.CreateAsync(service).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);
            return OperationResult.Complete();
        }

        public new async Task<DataService?> GetItemAsync(int id)
        {
            var service = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return service ?? throw new KeyNotFoundException(_localizer[nameof(DataServiceResources.ServiceNotFound)]);
        }

        public async Task<PagedResult<DataService>> GetListAsync(DataServiceFilter serviceFilter)
        {
            serviceFilter ??= new DataServiceFilter { Name = string.Empty };
            var result = await Repository.GetListAsync(serviceFilter).ConfigureAwait(false);
            if (result == null || !result.Result.Any())
            {
                throw new KeyNotFoundException(_localizer[nameof(DataServiceResources.ServicesNoFound)]);
            }
            return result;
        }

        public override async Task<OperationResult> UpdateAsync(DataService service)
        {
            await Repository.UpdateAsync(service).ConfigureAwait(false);
            return OperationResult.Complete();
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            if (!await Repository.VerifyServiceExistsAsync(id).ConfigureAwait(false))
            {
                return OperationResult.NotFound(_localizer[nameof(DataServiceResources.ServiceNotFound)]);
            }
            await Repository.DeleteAsync(id).ConfigureAwait(false);
            return OperationResult.Complete();
        }

        public async Task<bool> VerifyServiceExistsAsync(int id)
        {
            if (await Repository.VerifyServiceExistsAsync(id).ConfigureAwait(false))
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

            if (await Repository.VerifyNameAlreadyExistsAsync(name).ConfigureAwait(false))
            {
                throw new ArgumentException(_localizer[nameof(DataServiceResources.ServiceAlreadyExists)]);
            }
            return false;
        }
    }
}