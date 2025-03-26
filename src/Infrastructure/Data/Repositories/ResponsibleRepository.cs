using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class ResponsibleRepository : RepositoryBase<Responsible, Context>, IResponsibleRepository
    {
        public ResponsibleRepository(Context context) : base(context)
        {
        }

        public async Task<PagedResult<Responsible>> GetListAsync(ResponsibleFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            IQueryable<Responsible> query = Context.Set<Responsible>();

            if (!string.IsNullOrEmpty(filter.Email))
            {
                query = query.Where(r => r.Email.Contains(filter.Email)); // Filtro exato por Email
            }

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(r => r.Name.Contains(filter.Name)); // Filtro exato por Nome
            }

            if (filter.AreaId != 0)
            {
                query = query.Where(r => r.AreaId == filter.AreaId); // Filtra pelo AreaId diretamente
            }

            return await GetPagedResultAsync(query, filter.Page, filter.PageSize).ConfigureAwait(false);
        }

        public async Task<bool> VerifyEmailAlreadyExistsAsync(string email)
        {
            return await Context.Set<Responsible>().AnyAsync(r => r.Email == email).ConfigureAwait(false);
        }

        private static async Task<PagedResult<Responsible>> GetPagedResultAsync(IQueryable<Responsible> query, int page, int pageSize)
        {
            var total = await query.CountAsync().ConfigureAwait(false);
            var result = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);

            return new PagedResult<Responsible>
            {
                Total = total,
                Result = result,
                Page = page,
                PageSize = pageSize
            };
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