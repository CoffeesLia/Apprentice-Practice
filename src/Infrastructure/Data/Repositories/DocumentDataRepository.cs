using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class DocumentDataRepository(Context context) : RepositoryBase<DocumentData, Context>(context), IDocumentRepository
    {
        public async Task<DocumentData?> GetByIdAsync(int id)
        {
            return await Context.Set<DocumentData>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {

            DocumentData? entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<DocumentData>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<PagedResult<DocumentData>> GetListAsync(DocumentDataFilter documentFilter)
        {
            ArgumentNullException.ThrowIfNull(documentFilter);

            var filters = PredicateBuilder.New<DocumentData>(true);
            documentFilter.Page = documentFilter.Page <= 0 ? 1 : documentFilter.Page;
            if (!string.IsNullOrWhiteSpace(documentFilter.Name))
                filters = filters.And(x => x.Name.Contains(documentFilter.Name));
            if (documentFilter.Url != null)
                filters = filters.And(x => x.Url == documentFilter.Url);
            if (documentFilter.ApplicationId > 0)
                filters = filters.And(x => x.ApplicationId == documentFilter.ApplicationId);

            return await GetListAsync(filter: filters, page: documentFilter.Page, sort: documentFilter.Sort, sortDir: documentFilter.SortDir, includeProperties: nameof(DocumentData.ApplicationData)
             ).ConfigureAwait(false);
        }


        public async Task<bool> IsDocumentNameUniqueAsync(string name, int applicationId, int? id = null)
        {
            return await Context.Set<DocumentData>()
                .AnyAsync(a => a.Name == name
                            && a.ApplicationId == applicationId
                            && (!id.HasValue || a.Id != id))
                .ConfigureAwait(false);
        }


        public async Task<bool> IsUrlUniqueAsync(Uri url, int applicationId, int? id = null)
        {
            return await Context.Set<DocumentData>().AnyAsync(a => a.Url == url
                           && a.ApplicationId == applicationId
                           && (!id.HasValue || a.Id != id)).ConfigureAwait(false);
        }
    }
}
