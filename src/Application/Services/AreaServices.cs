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
        private IStringLocalizer localizer => localizerFactory.Create(typeof(AreaResources));
        protected override IAreaRepository Repository => UnitOfWork.AreaRepository;

        public override async Task<OperationResult> CreateAsync(Area item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }
            if (!await IsAreaNameUniqueAsync(item.Name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(localizer[nameof(AreaResources.AlreadyExists)]);
            }
            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<Area>> GetListAsync(AreaFilter areaFilter)
        {
            areaFilter ??= new AreaFilter();
            return await Repository.GetListAsync(areaFilter).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            if (await Repository.VerifyAplicationsExistsAsync(id).ConfigureAwait(false))
            {
                return OperationResult.Conflict(localizer[nameof(AreaResources.Undeleted)]);
            }
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var area = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return area != null
                ? OperationResult.Complete()
                : OperationResult.NotFound(localizer[nameof(ServiceResources.NotFound)]);
        }


        public async Task<bool> IsAreaNameUniqueAsync(string name, int? id = null)
        {
            var filter = new AreaFilter { Name = name };
            var areas = await GetListAsync(filter).ConfigureAwait(false);

            if (areas == null || areas.Result == null)
            {
                return true; 
            }

            return !areas.Result.Any(a => a.Id != id);
        }

        public override async Task<OperationResult> UpdateAsync(Area area)
        {
            ArgumentNullException.ThrowIfNull(area);

            var validationResult = await Validator.ValidateAsync(area).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            var existingArea = await Repository.GetByIdAsync(area.Id).ConfigureAwait(false);
            if (existingArea == null)
            {
                return OperationResult.NotFound(localizer[nameof(AreaResources.NotFound)]);
            }

            if (await Repository.VerifyNameAlreadyExistsAsync(area.Name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(localizer[nameof(AreaResources.AlreadyExists)]);
            }
            return await base.UpdateAsync(area).ConfigureAwait(false);
        }
    }
}
