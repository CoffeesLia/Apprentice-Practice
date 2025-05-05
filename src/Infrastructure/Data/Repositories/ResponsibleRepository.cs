using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using LinqKit;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class ResponsibleRepository : RepositoryBase<Responsible, Context>, IResponsibleRepository
    {
        public ResponsibleRepository(Context context) : base(context)
        {
        }
        public async Task<PagedResult<Responsible>> GetListAsync(ResponsibleFilter responsibleFilter)
        {
            ArgumentNullException.ThrowIfNull(responsibleFilter);

            var filters = PredicateBuilder.New<Responsible>(true);
            responsibleFilter.Page = responsibleFilter.Page <= 0 ? 1 : responsibleFilter.Page;

            if (!string.IsNullOrWhiteSpace(responsibleFilter.Email))
                filters = filters.And(x => x.Email.Contains(responsibleFilter.Email));
            if (!string.IsNullOrWhiteSpace(responsibleFilter.Name))
                filters = filters.And(x => x.Name.Contains(responsibleFilter.Name));
            if (responsibleFilter.AreaId != 0)
                filters = filters.And(x => x.AreaId == responsibleFilter.AreaId);

            return await GetListAsync(filter: filters, page: responsibleFilter.Page, sort: responsibleFilter.Sort, sortDir: responsibleFilter.SortDir).ConfigureAwait(false);
        }

        public async Task<bool> VerifyEmailAlreadyExistsAsync(string email)
        {
            return await Context.Set<Responsible>().AnyAsync(r => r.Email == email).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            var entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<Responsible>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<Responsible?> GetByIdAsync(int id)
        {
            return await Context.Set<Responsible>().FindAsync(id).ConfigureAwait(false);
        }


    }
}