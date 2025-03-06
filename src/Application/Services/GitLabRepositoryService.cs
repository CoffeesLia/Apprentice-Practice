
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using FluentValidation.Results;
using Stellantis.ProjectName.Application.Models.Filters;

namespace Stellantis.ProjectName.Domain.Services
{
    public class GitLabRepositoryService : IGitLabRepositoryService
    {
        private const string V = "Name, Description, and URL are required fields.";
        private readonly List<EntityGitLabRep> _repositories = new List<EntityGitLabRep>();

        async Task<OperationResult> IGitLabRepositoryService.CreateAsync(EntityGitLabRep newRepo)
        {
            // Verificar se os campos obrigatórios estão preenchidos
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

            // Verificar se a URL já existe
            if (_repositories.Any(repo => repo.Url == newRepo.Url))
            {
                return OperationResult.Conflict("A repository with the same URL already exists.");
            }

            // Adicionar o novo repositório à lista
            _repositories.Add(newRepo);
            return OperationResult.Complete("Repository created successfully.");
        }

        public async Task<EntityGitLabRep?> GetRepositoryDetailsAsync(int id)
        {
            var repository = _repositories.FirstOrDefault(repo => repo.Id == id);
            if (repository == null)
            {
                return null;
            }

            return repository;
        }

        public async Task<OperationResult> UpdateAsync(EntityGitLabRep updatedRepo, string v)
        {
            var existingRepo = _repositories.FirstOrDefault(repo => repo.Id == updatedRepo.Id);
            if (existingRepo == null)
            {
                return OperationResult.NotFound("Repository not found.");
            }

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

            if (_repositories.Any(repo => repo.Url == updatedRepo.Url && repo.Id != updatedRepo.Id))
            {
                return OperationResult.Conflict("A repository with the same URL already exists.");
            }

            existingRepo.Name = updatedRepo.Name;
            existingRepo.Description = updatedRepo.Description;
            existingRepo.Url = updatedRepo.Url;
            existingRepo.ApplicationId = updatedRepo.ApplicationId;

            return OperationResult.Complete("Repository updated successfully.");
        }

        // Implementação dos métodos não implementados
        public async Task<OperationResult> CreateAsync(EntityGitLabRep item)
        {
            return await ((IGitLabRepositoryService)this).CreateAsync(item);
        }

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

        public async Task<EntityGitLabRep?> GetItemAsync(int id)
        {
            return await GetRepositoryDetailsAsync(id);
        }

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

        public async IAsyncEnumerable<EntityGitLabRep> ListRepositories()
        {
            foreach (var repo in _repositories)
            {
                yield return repo;
                await Task.Yield(); // Permite que o método seja assíncrono
            }
        }

        Task<OperationResult> IEntityServiceBase<EntityGitLabRep>.UpdateAsync(EntityGitLabRep item)
        {
            return UpdateAsync(item, V);
        }
    }
}
