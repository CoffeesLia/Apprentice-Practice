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

        public async override Task<OperationResult> CreateAsync(Area item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var result = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!result.IsValid)
                return OperationResult.InvalidData(result);

            var existItem = await Repository.GetByNameAsync(item.Name).ConfigureAwait(false);
            if (existItem == null)
                return await base.CreateAsync(item).ConfigureAwait(false);

            return OperationResult.Conflict(_localizer[nameof(AreaResources.AlreadyExists)]);
        }

        public async override Task<OperationResult> DeleteAsync(int id)
        {
            if (await Repository.HasApplicationsAsync(id).ConfigureAwait(false))
                return OperationResult.Conflict(_localizer[nameof(AreaResources.Undeleted)]);
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<Area>> GetListAsync(AreaFilter? filter = null)
        {
            return await Repository.GetListAsync(filter ?? new AreaFilter()).ConfigureAwait(false);
        }

        public async override Task<OperationResult> UpdateAsync(Area item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var result = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!result.IsValid)
                return OperationResult.InvalidData(result);

            var existItem = await Repository.GetByNameAsync(item.Name).ConfigureAwait(false);
            if (existItem == null)
                return await base.UpdateAsync(item).ConfigureAwait(false);

            return OperationResult.Conflict(_localizer[AreaResources.AlreadyExists]);
        }
    }
}