using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories

{
    public class GitRepoRepository(Context context) : RepositoryBase<GitRepo, Context>(context), IGitRepoRepository
    {
        private readonly Context _context = context;

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                _context.Set<GitRepo>().Remove(entity);
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
                _context.Set<GitRepo>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<GitRepo?> GetByIdAsync(int id)
        {
            return await _context.Set<GitRepo>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<GitRepo?> GetRepositoryDetailsAsync(int id)
        {
            return await GetByIdAsync(id).ConfigureAwait(false);
        }

        public async IAsyncEnumerable<GitRepo> ListRepositories()
        {
            await foreach (var repo in _context.Set<GitRepo>().AsAsyncEnumerable().ConfigureAwait(false))
            {
                yield return repo;
            }
        }

        public async Task<OperationResult> CreateAsync(GitRepo newRepo)
        {
            try
            {
                await _context.Set<GitRepo>().AddAsync(newRepo).ConfigureAwait(false);
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

        public async Task<bool> IsApplicationDataFrom(int applicationDataId, int areaId)
        {
            return await _context.Set<ApplicationData>()
                .AnyAsync(ad => ad.Id == applicationDataId && ad.AreaId == areaId).ConfigureAwait(false);
        }

        public async Task<bool> VerifyUrlAlreadyExistsAsync(Uri url)
        {
            return await _context.Set<GitRepo>().AnyAsync(a => a.Url == url).ConfigureAwait(false);
        }

        public async Task<bool> VerifyAplicationsExistsAsync(int id)
        {
            return await _context.Set<GitRepo>().AnyAsync(repo => repo.ApplicationId == id).ConfigureAwait(false);
        }

        async Task IRepositoryEntityBase<GitRepo>.DeleteAsync(int id, bool saveChanges)
        {
            await DeleteAsync(id, saveChanges).ConfigureAwait(false);
        }

        public Task<bool> VerifyNameAlreadyExistsAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<GitRepo>> GetListAsync(GitRepoFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var filters = PredicateBuilder.New<GitRepo>(true);

            if (!string.IsNullOrWhiteSpace(filter.Description))
                filters = filters.And(x => x.Description.Contains(filter.Description));
            if (!string.IsNullOrWhiteSpace(filter.Name))
                filters = filters.And(x => x.Name.Contains(filter.Name));
            if (filter.Url != null)
                filters = filters.And(x => x.Url == filter.Url);

            return await GetListAsync(filter: filters, page: filter.Page, sort: filter.Sort, sortDir: filter.SortDir).ConfigureAwait(false);
        }


    }
}