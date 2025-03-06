using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using FluentValidation.Results;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Filters;

namespace Stellantis.ProjectName.Domain.Services
{
    // Implementation of the GitLab repository service
    public class GitLabRepositoryService : IGitLabRepositoryService, IEntityServiceBase<EntityGitLabRep>
    {
        private const string V = "Name, Description, and URL are required fields.";
        private readonly List<EntityGitLabRep> _repositories = new List<EntityGitLabRep>();

        // Method to create a new GitLab repository
        async Task<OperationResult> IGitLabRepositoryService.CreateAsync(EntityGitLabRep newRepo)
        {
            // Verify if the required fields are filled
            if (string.IsNullOrWhiteSpace(newRepo.Name) || string.IsNullOrWhiteSpace(newRepo.Description) || string.IsNullOrWhiteSpace(newRepo.Url))
            {
                var failures = new List<ValidationFailure>
                                    {
                                        new ValidationFailure("Name", "Name is required"),
                                        new ValidationFailure("Description", "Description is required"),
                                        new ValidationFailure("Url", "URL is required")
                                    };
                return OperationResult.InvalidData(new ValidationResult(failures));
            }

            // Verify if the URL already exists
            if (_repositories.Any(repo => repo.Url == newRepo.Url))
            {
                return OperationResult.Conflict("A repository with the same URL already exists.");
            }

            // Add the new repository to the list
            _repositories.Add(newRepo);
            return OperationResult.Complete("Repository created successfully.");
        }

        // Method to get repository details by ID
        public async Task<EntityGitLabRep?> GetRepositoryDetailsAsync(int id)
        {
            var repository = _repositories.FirstOrDefault(repo => repo.Id == id);
            if (repository == null)
            {
                return null;
            }

            return repository;
        }

        // Method to update an existing repository
        public async Task<OperationResult> UpdateAsync(EntityGitLabRep updatedRepo, string v)
        {
            var existingRepo = _repositories.FirstOrDefault(repo => repo.Id == updatedRepo.Id);
            if (existingRepo == null)
            {
                return OperationResult.NotFound("Repository not found.");
            }

            // Verify if the required fields are filled
            if (string.IsNullOrWhiteSpace(updatedRepo.Name) || string.IsNullOrWhiteSpace(updatedRepo.Description) || string.IsNullOrWhiteSpace(updatedRepo.Url))
            {
                var failures = new List<ValidationFailure>
                                    {
                                        new ValidationFailure("Name", "Name is required"),
                                        new ValidationFailure("Description", "Description is required"),
                                        new ValidationFailure("Url", "URL is required")
                                    };
                return OperationResult.InvalidData(new ValidationResult(failures));
            }

            // Verify if the URL is unique within the repositories list, excluding the current repository
            if (_repositories.Any(repo => repo.Url == updatedRepo.Url && repo.Id != updatedRepo.Id))
            {
                return OperationResult.Conflict("A repository with the same URL already exists.");
            }

            // Update the data of the existing repository
            existingRepo.Name = updatedRepo.Name;
            existingRepo.Description = updatedRepo.Description;
            existingRepo.Url = updatedRepo.Url;
            existingRepo.ApplicationId = updatedRepo.ApplicationId;

            return OperationResult.Complete("Repository updated successfully.");
        }

        // Method to create a new repository (interface implementation)
        public async Task<OperationResult> CreateAsync(EntityGitLabRep item)
        {
            return await ((IGitLabRepositoryService)this).CreateAsync(item);
        }

        // Method to delete a repository by ID
        public async Task<OperationResult> DeleteAsync(int id)
        {
            var existingRepo = _repositories.FirstOrDefault(repo => repo.Id == id);
            if (existingRepo == null)
            {
                return OperationResult.NotFound("Repository not found.");
            }

            _repositories.Remove(existingRepo);
            return OperationResult.Complete("Repository deleted successfully.");
        }

        // Method to get a repository by ID (interface implementation)
        public async Task<EntityGitLabRep?> GetItemAsync(int id)
        {
            return await GetRepositoryDetailsAsync(id);
        }

        // Method to get a paginated list of repositories based on a filter
        public async Task<PagedResult<EntityGitLabRep>> GetListAsync(GitLabFilter filter)
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

            var result = new PagedResult<EntityGitLabRep>
            {
                Result = filteredRepos.ToList(),
                Page = 1,
                PageSize = filteredRepos.Count(),
                Total = filteredRepos.Count()
            };

            return await Task.FromResult(result);
        }

        // Method to list all repositories asynchronously
        public async IAsyncEnumerable<EntityGitLabRep> ListRepositories()
        {
            foreach (var repo in _repositories)
            {
                yield return repo;
                await Task.Yield(); // Allows the method to be asynchronous
            }
        }

        // Method to update a repository (interface implementation)
        Task<OperationResult> IEntityServiceBase<EntityGitLabRep>.UpdateAsync(EntityGitLabRep item, string v)
        {
            return UpdateAsync(item, V);
        }

        // Method not implemented to update an area (required to fulfill the interface)
        public Task UpdateAsync(Area area)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> UpdateAsync(EntityGitLabRep item)
        {
            throw new NotImplementedException();
        }
    }
}