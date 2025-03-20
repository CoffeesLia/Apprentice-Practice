using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using LinqKit;
using Stellantis.ProjectName.Application.Resources;
using System.Data.Entity;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories

{
    public class GitRepoRepository : RepositoryBase<GitRepo, Context>, IGitRepoRepository
    {
        public GitRepoRepository(Context context) : base(context)
        {
        }

        public async Task<bool> VerifyAplicationsExistsAsync(int id)
        {
            return await Context.Set<GitRepo>().AnyAsync(a => a.Id == id).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
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

        public Task<PagedResult<GitRepo>> GetListAsync(GitLabFilter filter)
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
            catch (Exception ex)
            {
                return OperationResult.NotFound(ex.Message);
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<GitRepo, bool>> expression)
        {
            return await Context.Set<GitRepo>().AnyAsync(expression).ConfigureAwait(false);
        }
    }
}

