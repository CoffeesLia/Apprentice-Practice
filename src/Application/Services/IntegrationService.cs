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
    public class IntegrationService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Integration> validator)
               : EntityServiceBase<Integration>(unitOfWork, localizerFactory, validator), IIntegrationService
    {
        protected override IIntegrationRepository Repository => UnitOfWork.IntegrationRepository;

        public override async Task<OperationResult> CreateAsync(Integration item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }


            if (await Repository.VerifyNameExistsAsync(item.Name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(Localizer[IntegrationResources.NameIsRequired]);
            }

            if (await Repository.VerifyDescriptionExistsAsync(item.Description).ConfigureAwait(false))
            {
                return OperationResult.Conflict(Localizer[IntegrationResources.DescriptionIsRequired]);
            }

            await Repository.CreateAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[IntegrationResources.MessageSucess]);
        }

        public new async Task<OperationResult> DeleteAsync(int id)
        {
            var integration = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (integration == null)
            {
                return OperationResult.NotFound(Localizer[IntegrationResources.MessageNotFound]);
            }

            await Repository.DeleteAsync(id).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[IntegrationResources.DeletedSuccessfully]);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var integration = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (integration == null)
            {
                return OperationResult.NotFound(Localizer[IntegrationResources.MessageNotFound]);
            }
            return OperationResult.Complete(Localizer[IntegrationResources.MessageSucess]);
        }

        public override async Task<OperationResult> UpdateAsync(Integration item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (await Repository.VerifyDescriptionExistsAsync(item.Description).ConfigureAwait(false))
            {
                return OperationResult.InvalidData(validationResult);
            }
            if (await Repository.VerifyNameExistsAsync(item.Name).ConfigureAwait(false))
            {
                return OperationResult.InvalidData(validationResult);
            }
            if (!await Repository.VerifyApplicationIdExistsAsync(item.ApplicationDataId).ConfigureAwait(false))
            {
                return OperationResult.Conflict(Localizer[IntegrationResources.ApplicationIsRequired]);
            }

            var existingIntegration = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingIntegration == null)
            {
                return OperationResult.NotFound(Localizer[IntegrationResources.MessageNotFound]);
            }
            await Repository.UpdateAsync(existingIntegration).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[IntegrationResources.UpdatedSuccessfully]);
        }

        public async Task<bool> IsIntegrationNameUniqueAsync(string name)
        {
            var filter = new IntegrationFilter
            {
                Name = name
            };
            var integration = await GetListAsync(filter).ConfigureAwait(false);

            if (integration == null || integration.Result == null)
            {
                return true;
            }

            return !integration.Result.Any(a => a.Name == name);
        }

        public async Task<PagedResult<Integration>> GetListAsync(IntegrationFilter filter)
        {
            filter ??= new IntegrationFilter();
            var integrationFilter = new IntegrationFilter
            {
                Name = filter.Name,
                ApplicationDataId = filter.ApplicationDataId,
                Page = filter.Page,
                PageSize = filter.PageSize
            };

            return await UnitOfWork.IntegrationRepository.GetListAsync(integrationFilter).ConfigureAwait(false);
        }
    }
}
