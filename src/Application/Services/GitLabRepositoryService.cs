
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Filters;
using FluentValidation.Results;

namespace Stellantis.ProjectName.Domain.Services
{
    public class GitLabRepositoryService : IGitLabRepositoryService
    {
        private const string ValidationErrorMessage = "Name, Description, and URL are required fields.";
        private readonly List<EntityGitLabRepository> _repositories = new List<EntityGitLabRepository>();

        async Task<OperationResult> IGitLabRepositoryService.CreateAsync(EntityGitLabRepository newRepo)
        {
            if (IsInvalidRepository(newRepo, out var validationResult))
            {
                return OperationResult.InvalidData("Invalid data", validationResult);
            }

            if (RepositoryUrlExists(newRepo.Url))
            {
                return OperationResult.Conflict(GitLabResource.ExistentRepositoryUrl);
            }

            _repositories.Add(newRepo);
            return OperationResult.Complete(GitLabResource.RegisteredSuccessfully);
        }

        public async Task<EntityGitLabRepository?> GetRepositoryDetailsAsync(int id)
        {
            return _repositories.FirstOrDefault(repo => repo.Id == id);
        }

        public async Task<OperationResult> UpdateAsync(EntityGitLabRepository updatedRepo, string v)
        {
            var existingRepo = _repositories.FirstOrDefault(repo => repo.Id == updatedRepo.Id);
            if (existingRepo == null)
            {
                return OperationResult.NotFound(GitLabResource.RepositoryNotFound);
            }

            if (IsInvalidRepository(updatedRepo, out var validationResult))
            {
                return OperationResult.InvalidData(validationResult.ToString(), validationResult);
            }

            if (RepositoryUrlExists(updatedRepo.Url, updatedRepo.Id))
            {
                return OperationResult.Conflict(GitLabResource.ExistentRepositoryUrl);
            }

            UpdateRepository(existingRepo, updatedRepo);
            return OperationResult.Complete(GitLabResource.UpdatedSuccessfully);
        }

        public async Task<OperationResult> CreateAsync(EntityGitLabRepository item)
        {
            return await ((IGitLabRepositoryService)this).CreateAsync(item);
        }

        public async Task<OperationResult> DeleteAsync(int id)
        {
            var existingRepo = _repositories.FirstOrDefault(repo => repo.Id == id);
            if (existingRepo == null)
            {
                return OperationResult.NotFound(GitLabResource.RepositoryNotFound);
            }

            _repositories.Remove(existingRepo);
            return OperationResult.Complete(GitLabResource.DeletedSuccessfully);
        }

        public async Task<EntityGitLabRepository?> GetItemAsync(int id)
        {
            return await GetRepositoryDetailsAsync(id);
        }

        public async Task<PagedResult<EntityGitLabRepository>> GetListAsync(GitLabFilter filter)
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

            var result = new PagedResult<EntityGitLabRepository>
            {
                Result = filteredRepos.ToList(),
                Page = 1,
                PageSize = filteredRepos.Count(),
                Total = filteredRepos.Count()
            };

            return await Task.FromResult(result);
        }

        public async IAsyncEnumerable<EntityGitLabRepository> ListRepositories()
        {
            foreach (var repo in _repositories)
            {
                yield return repo;
                await Task.Yield();
            }
        }

        Task<OperationResult> IEntityServiceBase<EntityGitLabRepository>.UpdateAsync(EntityGitLabRepository item)
        {
            return UpdateAsync(item, ValidationErrorMessage);
        }

        private bool IsInvalidRepository(EntityGitLabRepository repo, out ValidationResult validationResult)
        {
            var failures = new List<ValidationFailure>();

            if (string.IsNullOrWhiteSpace(repo.Name))
            {
                failures.Add(new ValidationFailure("Name", GitLabResource.Name));
            }

            if (string.IsNullOrWhiteSpace(repo.Description))
            {
                failures.Add(new ValidationFailure("Description", GitLabResource.Description));
            }

            if (string.IsNullOrWhiteSpace(repo.Url))
            {
                failures.Add(new ValidationFailure("Url", GitLabResource.Url));
            }

            validationResult = new ValidationResult(failures);
            return failures.Any();
        }

        private bool RepositoryUrlExists(string url, int? id = null)
        {
            return _repositories.Any(repo => repo.Url == url && (!id.HasValue || repo.Id != id.Value));
        }

        private void UpdateRepository(EntityGitLabRepository existingRepo, EntityGitLabRepository updatedRepo)
        {
            existingRepo.Name = updatedRepo.Name;
            existingRepo.Description = updatedRepo.Description;
            existingRepo.Url = updatedRepo.Url;
            existingRepo.ApplicationId = updatedRepo.ApplicationId;
        }
    }
}
