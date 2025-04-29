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
    public class ApplicationDataService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<ApplicationData> validator)
            : EntityServiceBase<ApplicationData>(unitOfWork, localizerFactory, validator), IApplicationDataService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(ApplicationDataResources));

        protected override IApplicationDataRepository Repository =>
            UnitOfWork.ApplicationDataRepository;


        public override async Task<OperationResult> CreateAsync(ApplicationData item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(item.Name);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (!await IsApplicationNameUniqueAsync(item.Name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.AlreadyExists)]);
            }

            if (!await IsResponsibleFromArea(item.AreaId, item.ResponsibleId).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.NotFound)]);
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var applicationData = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (applicationData == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ApplicationDataResources.ApplicationNotFound)]);
            }
            var result = new
            {
                applicationData.Name,
                applicationData.Area
            };
            return OperationResult.Complete(result.ToString() ?? string.Empty);
        }

        public async Task<bool> IsApplicationNameUniqueAsync(string name, int? id = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var existingItems = await Repository.GetListAsync(new ApplicationFilter { Name = name }).ConfigureAwait(false);
            if (existingItems?.Result == null)
            {
                return true;
            }

            return !existingItems.Result.Any(e => e.Id != id);
        }

        public override async Task<OperationResult> UpdateAsync(ApplicationData item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(item.Name);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (!await IsApplicationNameUniqueAsync(item.Name, item.Id).ConfigureAwait(false))
            {
                return OperationResult.Conflict(ApplicationDataResources.AlreadyExists);
            }

            if (!await IsResponsibleFromArea(item.AreaId, item.ResponsibleId).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.NotFound)]);
            }

            return await base.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter)
        {
            applicationFilter ??= new ApplicationFilter();
            return await UnitOfWork.ApplicationDataRepository.GetListAsync(applicationFilter).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetFullByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.NotFound(base.Localizer[nameof(ApplicationDataResources.ApplicationNotFound)]);
            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

        public async Task<bool> IsResponsibleFromArea(int areaId, int responsibleId)
        {
            var responsible = await UnitOfWork.ResponsibleRepository.GetByIdAsync(responsibleId).ConfigureAwait(false);

            if (responsible == null)
            {
                return false;
            }

            return responsible.AreaId == areaId;
        }

    }
}