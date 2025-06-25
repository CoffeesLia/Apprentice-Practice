using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class RepoRepository(Context context) : RepositoryBase<Repo, Context>(context), IRepoRepository
    {
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


        public async Task<Repo?> GetByIdAsync(int id)
        {
            return await Context.Set<Repo>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<Repo>> GetListAsync(RepoFilter repofilter)
        {
            ArgumentNullException.ThrowIfNull(repofilter);

            var filters = PredicateBuilder.New<Repo>(true);
            repofilter.Page = repofilter.Page <= 0 ? 1 : repofilter.Page;
            if (!string.IsNullOrWhiteSpace(repofilter.Name))
                filters = filters.And(x => x.Name.Contains(repofilter.Name));
            if (repofilter.Url != null)
                filters = filters.And(x => x.Url == repofilter.Url);
            if (repofilter.ApplicationId > 0)
                filters = filters.And(x => x.ApplicationId == repofilter.ApplicationId);

            return await GetListAsync(filter: filters, page: repofilter.Page, sort: repofilter.Sort, sortDir: repofilter.SortDir, includeProperties: nameof(Repo.ApplicationData)
             ).ConfigureAwait(false);
        }

        public async Task<bool> IsRepoNameUniqueAsync(string Name, int applicationId, int? id = null)
        {
            return await Context.Set<Repo>()
                .AnyAsync(a => a.Name == Name
                            && a.ApplicationId == applicationId
                            && (!id.HasValue || a.Id != id))
                .ConfigureAwait(false);
        }


        public async Task<bool> IsUrlUniqueAsync(Uri url, int applicationId, int? id = null)
        {
            return await Context.Set<Repo>().AnyAsync(a => a.Url == url
                           && a.ApplicationId == applicationId
                           && (!id.HasValue || a.Id != id)).ConfigureAwait(false);
        }
        public async Task<bool> VerifyDescriptionExistsAsync(string description)
        {
            return await Context.Set<Repo>().AnyAsync(repo => repo.Description == description).ConfigureAwait(false);
        }
    }
}