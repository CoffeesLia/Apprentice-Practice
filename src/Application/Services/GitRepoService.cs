using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using FluentValidation;

namespace Stellantis.ProjectName.Application.Services
{
    public class GitRepoService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<GitRepo> validator)
            : EntityServiceBase<GitRepo>(unitOfWork, localizerFactory, validator), IGitRepoService
    {
        private new IStringLocalizer Localizer => localizerFactory.Create(typeof(GitResource));
        

        protected override IGitRepoRepository Repository => UnitOfWork.GitRepoRepository;

    

public override async Task<OperationResult> CreateAsync(GitRepo item)
        {
       
            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);

            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }
            if (await Repository.VerifyUrlAlreadyExistsAsync(item.Url).ConfigureAwait(false))
            {
                throw new InvalidOperationException(Localizer[nameof(GitResource.ExistentRepositoryUrl)]);
            }

            await Repository.CreateAsync(item).ConfigureAwait(true);
            return OperationResult.Complete(Localizer[nameof(ServiceResources.RegisteredSuccessfully)]);
        }

        public async Task<bool> VerifyAplicationsExistsAsync(int id)
        {
            return await Repository.AnyAsync(a => a.Id == id).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
            {
                return OperationResult.NotFound(Localizer[nameof(ServiceResources.NotFound)]);
            }

            await Repository.DeleteAsync(id).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[nameof(ServiceResources.DeletedSuccessfully)]);
        }
    }

}

