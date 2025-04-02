using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class SquadRepository(Context context) : RepositoryEntityBase<Squad, Context>(context), ISquadRepository
    {
        private readonly Context _context = context;

        public new async Task CreateAsync(Squad squad, bool saveChanges = true)
        {
            await base.CreateAsync(squad, saveChanges).ConfigureAwait(false);
        }

        public async Task<Squad?> GetByIdAsync(Guid id)
        {
            return await _context.Squads.FindAsync(id).ConfigureAwait(false);
        }

        public new async Task UpdateAsync(Squad squad, bool saveChanges = true)
        {
            await base.UpdateAsync(squad, saveChanges).ConfigureAwait(false);
        }

        public new async Task DeleteAsync(int id, bool saveChanges = true)
        {
            var squad = await _context.Squads.FindAsync(id).ConfigureAwait(false);
            if (squad != null)
            {
                await base.DeleteAsync(squad, saveChanges).ConfigureAwait(false);
            }
        }

        public async Task<bool> VerifyNameAlreadyExistsAsync(string name)
        {
            return await _context.Squads.AnyAsync(s => s.Name == name).ConfigureAwait(false);
        }

        public async Task<PagedResult<Squad>> GetListAsync(SquadFilter squadFilter)
        {
            ArgumentNullException.ThrowIfNull(squadFilter);

            var query = _context.Squads.AsQueryable();

            if (!string.IsNullOrEmpty(squadFilter.Name))
            {
                query = query.Where(s => s.Name != null && s.Name.Contains(squadFilter.Name));
            }

            return await GetListAsync(query, squadFilter.Sort, squadFilter.SortDir, squadFilter.Page, squadFilter.PageSize).ConfigureAwait(false);
        }

        public async Task<bool> VerifySquadExistsAsync(int id)
        {
            return await _context.Squads.AnyAsync(s => s.Id == id).ConfigureAwait(false);
        }
    }
}
