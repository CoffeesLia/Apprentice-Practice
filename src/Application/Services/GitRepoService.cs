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
    public class GitRepoService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<GitRepo> validator) : EntityServiceBase<GitRepo>(unitOfWork, localizerFactory, validator), IGitRepoService
    {
        private new IStringLocalizer Localizer => localizerFactory.Create(typeof(GitResource));
        private readonly IStringLocalizerFactory localizerFactory = localizerFactory;

        protected override IGitRepoRepository Repository => UnitOfWork.GitRepoRepository;

        public override async Task<OperationResult> CreateAsync(GitRepo item)
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

            if (await Repository.VerifyUrlAlreadyExistsAsync(item.Url).ConfigureAwait(false))
            {
                return OperationResult.InvalidData(validationResult);
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public async Task<bool> VerifyAplicationsExistsAsync(int id)
        {
            return await Repository.GetByIdAsync(id).ConfigureAwait(false) != null;
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var responsible = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return responsible != null
                ? OperationResult.Complete()
                : OperationResult.NotFound(Localizer[nameof(GitResource.RepositoryNotFound)]);
        }
        public override async Task<OperationResult> UpdateAsync(GitRepo item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);

            var existingRepo = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingRepo == null)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (await Repository.VerifyUrlAlreadyExistsAsync(item.Url).ConfigureAwait(false) && existingRepo.Url != item.Url)
            {
                return OperationResult.Conflict(Localizer[nameof(GitResource.AlreadyExists)]);
            }

            existingRepo.Name = item.Name;
            existingRepo.Description = item.Description;
            existingRepo.Url = item.Url;
            existingRepo.ApplicationId = item.ApplicationId;

            return await base.UpdateAsync(existingRepo).ConfigureAwait(false);
        }

        public async Task<PagedResult<GitRepo>> GetListAsync(GitRepoFilter gitRepoFilter)
        {
            gitRepoFilter ??= new GitRepoFilter
            {
                Name = string.Empty,
                Description = string.Empty,
                Url = new Uri($"http://example.com"),
            };
            return await UnitOfWork.GitRepoRepository.GetListAsync(gitRepoFilter).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
            {
                return OperationResult.Conflict(Localizer[nameof(GitResource.RepositoryNotFound)]);
            }
            return await base.DeleteAsync(item).ConfigureAwait(false);
        }
    }
}

