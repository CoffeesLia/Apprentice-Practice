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

        public async Task<PagedResult<Repo>> GetListAsync(RepoFilter documentFilter)
        {
            ArgumentNullException.ThrowIfNull(documentFilter);

            var filters = PredicateBuilder.New<Repo>(true);
            documentFilter.Page = documentFilter.Page <= 0 ? 1 : documentFilter.Page;
            if (!string.IsNullOrWhiteSpace(documentFilter.Name))
                filters = filters.And(x => x.Name.Contains(documentFilter.Name));
            if (documentFilter.Url != null)
                filters = filters.And(x => x.Url == documentFilter.Url);
            if (documentFilter.ApplicationId > 0)
                filters = filters.And(x => x.ApplicationId == documentFilter.ApplicationId);

            return await GetListAsync(filter: filters, page: documentFilter.Page, sort: documentFilter.Sort, sortDir: documentFilter.SortDir, includeProperties: nameof(Repo.ApplicationData)
             ).ConfigureAwait(false);
        }

        public async Task<bool> IsRepoNameUniqueAsync(string name, int applicationId, int? id = null)
        {
            return await Context.Set<Repo>()
                .AnyAsync(a => a.Name == name
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