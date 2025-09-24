using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class RepoRepository(Context context) : RepositoryBase<Repo, Context>(context), IRepoRepository
    {
        public async Task<Repo?> GetByIdAsync(int id)
        {
            return await Context.Set<Repo>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            Repo? entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<Repo>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<PagedResult<Repo>> GetListAsync(RepoFilter repoFilter)
        {
            ArgumentNullException.ThrowIfNull(repoFilter);

            var filters = PredicateBuilder.New<Repo>(true);
            repoFilter.Page = repoFilter.Page <= 0 ? 1 : repoFilter.Page;
            if (!string.IsNullOrWhiteSpace(repoFilter.Name))
                filters = filters.And(a => a.Name != null && a.Name.ToLower().Contains(repoFilter.Name.ToLower()));
            if (!string.IsNullOrWhiteSpace(repoFilter.Description))
                filters = filters.And(a => a.Description != null && a.Description.ToLower().Contains(repoFilter.Description.ToLower()));
            if (repoFilter.Url != null)
                filters = filters.And(x => x.Url == repoFilter.Url);
            if (repoFilter.ApplicationId > 0)
                filters = filters.And(x => x.ApplicationId == repoFilter.ApplicationId);

            return await GetListAsync(
                filter: filters,
                pageSize: repoFilter.PageSize,
                page: repoFilter.Page,
                sort: repoFilter.Sort,
                sortDir: repoFilter.SortDir,
                includeProperties: nameof(Repo.ApplicationData)
            ).ConfigureAwait(false);
        }

        public async Task<bool> NameAlreadyExists(string name, int? id = null)
        {
            return await Context.Set<Repo>()
                .AnyAsync(a => a.Name == name
                            && (!id.HasValue || a.Id != id))
                .ConfigureAwait(false);
        }

        public async Task<bool> UrlAlreadyExists(Uri url, int? id = null)
        {
            var query = Context.Set<Repo>().Where(r => r.Url == url);
            if (id.HasValue)
                query = query.Where(r => r.Id != id.Value);
            return await query.AnyAsync().ConfigureAwait(false);
        }
    }
}