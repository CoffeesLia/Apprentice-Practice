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
    public class DocumentDataService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<DocumentData> validator)
        : EntityServiceBase<DocumentData>(unitOfWork, localizerFactory, validator), IDocumentService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(DocumentDataResources));

        protected override IDocumentRepository Repository =>
            UnitOfWork.DocumentDataRepository;


        public override async Task<OperationResult> CreateAsync(DocumentData item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(item.Name);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (await Repository.IsDocumentNameUniqueAsync(item.Name!, item.ApplicationId).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(DocumentDataResources.NameAlreadyExists)]);
            }

            if (await Repository.IsUrlUniqueAsync(item.Url!, item.ApplicationId).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(DocumentDataResources.UrlAlreadyExists)]);
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(DocumentData item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(item.Name);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (await Repository.IsDocumentNameUniqueAsync(item.Name!, item.ApplicationId).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(DocumentDataResources.NameAlreadyExists)]);
            }

            if (await Repository.IsUrlUniqueAsync(item.Url!, item.ApplicationId).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(DocumentDataResources.UrlAlreadyExists)]);
            }

            return await base.UpdateAsync(item).ConfigureAwait(false);

        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<DocumentData>> GetListAsync(DocumentDataFilter filter)
        {
            filter ??= new DocumentDataFilter();

            return await UnitOfWork.DocumentDataRepository.GetListAsync(filter).ConfigureAwait(false);
        }
    }
}
