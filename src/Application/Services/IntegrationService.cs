using FluentValidation;
using FluentValidation.Results;
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
              return OperationResult.Conflict(Localizer[IntegrationResources.MessageConflict]);
            }

            if (await Repository.VerifyDescriptionExistsAsync(item.Description).ConfigureAwait(false))
            {
                return OperationResult.Conflict(Localizer[IntegrationResources.MessageConflict]);
            }

            await Repository.CreateAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[IntegrationResources.MessageSucess]);
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

            var existingItem = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingItem is null)
            {
                return OperationResult.NotFound(Localizer[IntegrationResources.MessageNotFound]);
            }

            UnitOfWork.IntegrationRepository.DetachEntity(existingItem);

            bool nameUnchanged = item.Name == existingItem.Name;
            bool descriptionUnchanged = item.Description == existingItem.Description;
            bool applicationDataIdUnchanged = item.ApplicationDataId == existingItem.ApplicationDataId;

            if (nameUnchanged && descriptionUnchanged && applicationDataIdUnchanged)
            {
                return OperationResult.InvalidData(new ValidationResult(
                [
                    new ValidationFailure(nameof(item.Name), Localizer[IntegrationResources.MessageConflict]),
                    new ValidationFailure(nameof(item.Description), Localizer[IntegrationResources.MessageConflict]),
                    new ValidationFailure(nameof(item.ApplicationDataId), Localizer[IntegrationResources.MessageConflict])
                ]));
            }

            await Repository.UpdateAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[IntegrationResources.UpdatedSuccessfully]);
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
