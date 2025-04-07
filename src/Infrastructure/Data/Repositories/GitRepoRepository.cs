using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories

{
    public class GitRepoRepository(Context context) : RepositoryBase<GitRepo, Context>(context), IGitRepoRepository
    {
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<GitRepo>().Remove(entity);
                await SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
            return false;
        }

        public async Task DeleteAsync(int id, bool saveChanges)
        {
            var entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<GitRepo>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<GitRepo?> GetByIdAsync(int id)
        {
            return await Context.Set<GitRepo>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<GitRepo?> GetRepositoryDetailsAsync(int id)
        {
            return await GetByIdAsync(id).ConfigureAwait(false);
        }

        public async IAsyncEnumerable<GitRepo> ListRepositories()
        {
            await foreach (var repo in Context.Set<GitRepo>().AsAsyncEnumerable().ConfigureAwait(false))
            {
                yield return repo;
            }
        }

        public Task<PagedResult<GitRepo>> GetListAsync(GitRepoFilter filter)
        {
            throw new NotImplementedException();
        }

        public async Task<OperationResult> CreateAsync(GitRepo newRepo)
        {
            try
            {
                await Context.Set<GitRepo>().AddAsync(newRepo).ConfigureAwait(false);
                await SaveChangesAsync().ConfigureAwait(false);
                return OperationResult.Complete();
            }
            catch (DbUpdateException ex)
            {
                return OperationResult.Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return OperationResult.Conflict(ex.Message); // Usando Conflict em vez de InvalidData
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is NotSupportedException)
            {
                return OperationResult.NotFound(ex.Message);
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<GitRepo, bool>> expression)
        {
            return await Context.Set<GitRepo>().AnyAsync(expression).ConfigureAwait(false);
        }

        public async Task<bool> VerifyUrlAlreadyExistsAsync(Uri url)
        {
            return await Context.Set<GitRepo>().AnyAsync(a => a.Url == url).ConfigureAwait(false);
        }

        public async Task<bool> VerifyAplicationsExistsAsync(int id)
        {
            return await Context.Set<GitRepo>().AnyAsync(repo => repo.ApplicationId == id).ConfigureAwait(false);
        }

        async Task IRepositoryEntityBase<GitRepo>.DeleteAsync(int id, bool saveChanges)
        {
            await DeleteAsync(id, saveChanges).ConfigureAwait(false);
        }

        public void GetListAysnc()
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyNameAlreadyExistsAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}