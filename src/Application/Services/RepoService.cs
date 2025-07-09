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
    public class RepoService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Repo> validator)
        : EntityServiceBase<Repo>(unitOfWork, localizerFactory, validator), IRepoService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(RepoResources));

        protected override IRepoRepository Repository =>
            UnitOfWork.RepoRepository;

        public override async Task<OperationResult> CreateAsync(Repo item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(item.Name);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (await Repository.NameAlreadyExists(item.Name!, item.ApplicationId).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(RepoResources.NameAlreadyExists)]);
            }

            if (await Repository.UrlAlreadyExists(item.Url!, item.ApplicationId).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(RepoResources.UrlAlreadyExists)]);
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(Repo item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(item.Name);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (await Repository.NameAlreadyExists(item.Name!, item.ApplicationId, item.Id).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(RepoResources.NameAlreadyExists)]);
            }

            if (await Repository.UrlAlreadyExists(item.Url!, item.ApplicationId, item.Id).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(RepoResources.UrlAlreadyExists)]);
            }

            return await base.UpdateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<Repo>> GetListAsync(RepoFilter filter)
        {
            filter ??= new RepoFilter();

            return await UnitOfWork.RepoRepository.GetListAsync(filter).ConfigureAwait(false);
        }
    }
}
