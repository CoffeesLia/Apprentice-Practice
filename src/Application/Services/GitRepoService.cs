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

        private readonly List<GitRepo> _repositories = new();


        public override async Task<OperationResult> CreateAsync(GitRepo item)
        {
            if (IsInvalidRepository(item, out var validationResult))
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (RepositoryUrlExists(item.Url))
            {
                return OperationResult.Conflict(GitResource.ExistentRepositoryUrl);
            }

            _repositories.Add(item);
            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public async Task<OperationResult> DeleteAsync(int id, GitRepo item)
        {
            var existingRepo = _repositories.FirstOrDefault(repo => repo.Id == id);
            if (existingRepo == null)
            {
                return OperationResult.NotFound(GitResource.RepositoryNotFound);
            }

            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<GitRepo>> GetListAsync(GitRepoFilter filter)
        {
            var filteredRepos = _repositories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                filteredRepos = filteredRepos.Where(repo => repo.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrWhiteSpace(filter.Description))
            {
                filteredRepos = filteredRepos.Where(repo => repo.Description.Contains(filter.Description));
            }

            if (!string.IsNullOrWhiteSpace(filter.Url))
            {
                filteredRepos = filteredRepos.Where(repo => repo.Url.Contains(filter.Url));
            }

            var result = new PagedResult<GitRepo>
            {
                Result = filteredRepos.ToList(),
                Page = 1,
                PageSize = filteredRepos.Count(),
                Total = filteredRepos.Count()
            };

            return await Task.FromResult(result);
        }

        public async IAsyncEnumerable<GitRepo> ListRepositories()
        {
            foreach (var repo in _repositories)
            {
                yield return repo;
                await Task.Yield();
            }
        }

        Task<OperationResult> IEntityServiceBase<GitRepo>.UpdateAsync(GitRepo item)
        {
            return Task.FromResult(OperationResult.Conflict(GitResource.ValidationErrorMessage));
        }

        private bool IsInvalidRepository(GitRepo repo, out ValidationResult validationResult)
        {
            var failures = new List<ValidationFailure>();

            if (string.IsNullOrWhiteSpace(repo.Name))
            {
                OperationResult.Complete(Localizer[nameof(GitResource.NameIsRequired)]);
            }

            if (string.IsNullOrWhiteSpace(repo.Description))
            {
                OperationResult.Complete(Localizer[nameof(GitResource.DescriptionIsRequired)]);
            }

            if (string.IsNullOrWhiteSpace(repo.Url))
            {
                OperationResult.Complete(Localizer[nameof(GitResource.UrlIsRequired)]);
            }

            validationResult = new ValidationResult(failures);
            return failures.Count != 0;
        }

        private bool RepositoryUrlExists(string url, int? id = null)
        {
            return _repositories.Any(repo => repo.Url == url && (!id.HasValue || repo.Id != id.Value));
        }

        private static void UpdateRepository(GitRepo existingRepo, GitRepo updatedRepo)
        {
            existingRepo.Name = updatedRepo.Name;
            existingRepo.Description = updatedRepo.Description;
            existingRepo.Url = updatedRepo.Url;
            existingRepo.ApplicationId = updatedRepo.ApplicationId;
        }

        public async Task<OperationResult> UpdateAsync(GitRepo updatedRepo)
        {
            var existingRepo = _repositories.FirstOrDefault(repo => repo.Id == updatedRepo.Id);
            if (existingRepo == null)
            {
                return OperationResult.NotFound(GitResource.RepositoryNotFound);
            }

            if (IsInvalidRepository(updatedRepo, out var validationResult))
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (RepositoryUrlExists(updatedRepo.Url, updatedRepo.Id))
            {
                return OperationResult.Conflict(GitResource.ExistentRepositoryUrl);
            }

            UpdateRepository(existingRepo, updatedRepo);
            return OperationResult.Complete(Localizer[ServiceResources.UpdatedSuccessfully]);
        }

        public async Task<bool> VerifyAplicationsExistsAsync(int id)
        {
            return await Repository.AnyAsync(a => a.Id == id).ConfigureAwait(false);
        }
    }

}

