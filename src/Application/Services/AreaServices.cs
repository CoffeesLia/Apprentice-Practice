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
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(AreaResources));
        protected override IAreaRepository Repository => UnitOfWork.AreaRepository;

        public override async Task<OperationResult> CreateAsync(Area item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            var manager = await UnitOfWork.ManagerRepository.GetByIdAsync(item.ManagerId).ConfigureAwait(false);
            if (manager == null)
            {
                return OperationResult.Conflict(_localizer[nameof(AreaResources.AreaInvalidManagerId)]);
            }

            var areasWithSameManager = await Repository.GetListAsync(new AreaFilter { ManagerId = item.ManagerId }).ConfigureAwait(false);
            if (areasWithSameManager?.Result?.Any() == true)
            {
                return OperationResult.Conflict(_localizer[nameof(AreaResources.ManagerUnavailable)]);
            }

            if (!await IsAreaNameUniqueAsync(item.Name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(AreaResources.AlreadyExists)]);
            }
            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var area = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return area != null
                ? OperationResult.Complete()
                : OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
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
                return OperationResult.NotFound(_localizer[nameof(AreaResources.NotFound)]);
            }

            var manager = await UnitOfWork.ManagerRepository.GetByIdAsync(area.ManagerId).ConfigureAwait(false);
            if (manager == null)
            {
                return OperationResult.InvalidData(validationResult);
            }

            var areasWithSameManager = await Repository.GetListAsync(new AreaFilter { ManagerId = area.ManagerId }).ConfigureAwait(false);
            if (areasWithSameManager.Result.Any(a => a.Id != area.Id))
            {
                return OperationResult.Conflict(_localizer[nameof(AreaResources.ManagerUnavailable)]);
            }

            var areasWithSameName = await Repository.GetListAsync(new AreaFilter { Name = area.Name }).ConfigureAwait(false);
            if (areasWithSameName.Result.Any(a => a.Id != area.Id))
            {
                return OperationResult.Conflict(_localizer[nameof(AreaResources.AlreadyExists)]);
            }

            return await base.UpdateAsync(area).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            if (await Repository.VerifyAplicationsExistsAsync(id).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(AreaResources.Undeleted)]);
            }
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<Area>> GetListAsync(AreaFilter areaFilter)
        {
            areaFilter ??= new AreaFilter();
            return await Repository.GetListAsync(areaFilter).ConfigureAwait(false);
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
    }
}
