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
    public class PartNumberService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory)
        : BaseEntityService<PartNumber, IPartNumberRepository>(unitOfWork, localizerFactory), IPartNumberService
    {
        private readonly IStringLocalizer _partNumberLocalizer = localizerFactory.Create(typeof(PartNumberResources));
        protected override IPartNumberRepository Repository => UnitOfWork.PartNumberRepository;

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetFullByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.Error(Localizer[nameof(GeneralResources.NotFound)]);
            else if (item.Suppliers.Count > 0 || item.Vehicles.Count > 0)
                return OperationResult.Error(_partNumberLocalizer[nameof(PartNumberResources.Undeleted)]);
            else
                return await base.DeleteAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> CreateAsync(PartNumber itemDto)
        {
            ArgumentNullException.ThrowIfNull(itemDto);
            itemDto = ValidateCreateOrUpdate(itemDto);
            if (Repository.VerifyCodeExists(itemDto.Code!))
                return OperationResult.Error(_partNumberLocalizer[nameof(PartNumberResources.AlreadyExistCode)]);
            return await base.CreateAsync(itemDto).ConfigureAwait(false);
        }

        public async Task<PagedResult<PartNumber>> GetListAysnc(PartNumberFilter filter)
        {
            return await UnitOfWork.PartNumberRepository.GetListAsync(filter).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(PartNumber item)
        {
            ArgumentNullException.ThrowIfNull(item);
            item = ValidateCreateOrUpdate(item);
            if (UnitOfWork.PartNumberRepository.VerifyCodeExists(item.Code))
                return OperationResult.Error(_partNumberLocalizer[nameof(PartNumberResources.AlreadyExistCode)]);
            return await base.UpdateAsync(item).ConfigureAwait(false);
        }

        private static PartNumber ValidateCreateOrUpdate(PartNumber partNumber)
        {
            if (partNumber.Code!.Length < 11)
                partNumber.Code = partNumber.Code.PadLeft(11, '0');
            return partNumber;
        }
    }
}
