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
    public class AreaService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Area> validator)
        : EntityServiceBase<Area>(unitOfWork, localizerFactory, validator), IAreaService
    {
        private IStringLocalizer _localizer => localizerFactory.Create(typeof(AreaResources));
        protected override IAreaRepository Repository => UnitOfWork.AreaRepository;

        public override async Task<OperationResult> CreateAsync(Area item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }
            if (await Repository.VerifyNameAlreadyExistsAsync(item.Name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(AreaResources.AlreadyExists)]);
            }
            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<Area>> GetListAsync(AreaFilter filter)
        {
            filter ??= new AreaFilter();
            return await Repository.GetListAsync(filter).ConfigureAwait(false);
        }

        public async Task<OperationResult> UpdateAreaAsync(Area area)
        {
            ArgumentNullException.ThrowIfNull(area);
            var validationResult = await Validator.ValidateAsync(area).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }
            if (await Repository.VerifyNameAlreadyExistsAsync(area.Name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(AreaResources.AlreadyExists)]);
            }
            return await base.UpdateAsync(area).ConfigureAwait(false);
        }

        public override async Task <OperationResult> DeleteAsync(int id)
        {
            if(await Repository.VerifyAplicationsExistsAsync(id).ConfigureAwait(false)) 
            {
                return OperationResult.Conflict(_localizer[nameof(AreaResources.Undeleted)]);
            }
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task<OperationResult> GetItemAsync(int id)
        {
            return await Repository.GetByIdAsync(id).ConfigureAwait(false) is Area area
                ? OperationResult.Complete()
                : OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
        }
    }
}