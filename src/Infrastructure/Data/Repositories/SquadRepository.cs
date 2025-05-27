using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class SquadRepository(Context context) : RepositoryEntityBase<Squad, Context>(context), ISquadRepository
    {
        public new async Task CreateAsync(Squad squad, bool saveChanges = true)
        {
            await base.CreateAsync(squad, saveChanges).ConfigureAwait(false);
        }

        public new async Task<Squad?> GetByIdAsync(int id)
        {
            return await Context.Squads.FindAsync(id).ConfigureAwait(false);
        }

        public new async Task UpdateAsync(Squad squad, bool saveChanges = true)
        {
            Context.Squads.Update(squad);

            if (saveChanges)
            {
                await Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public new async Task DeleteAsync(int id, bool saveChanges = true)
        {
            Squad? squad = await Context.Squads.FindAsync(id).ConfigureAwait(false);
            if (squad != null)
            {
                Context.Squads.Remove(squad);

                if (saveChanges)
                {
                    await Context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<bool> VerifyNameAlreadyExistsAsync(string name)
        {
            return await Context.Squads.AnyAsync(s => s.Name == name).ConfigureAwait(false);
        }

        public async Task<PagedResult<Squad>> GetListAsync(SquadFilter squadFilter)
        {
            ArgumentNullException.ThrowIfNull(squadFilter);

            // Garante que o valor de Page seja pelo menos 1
            squadFilter.Page = squadFilter.Page <= 0 ? 1 : squadFilter.Page;

            IQueryable<Squad> query = Context.Squads.AsQueryable();

            if (!string.IsNullOrEmpty(squadFilter.Name))
            {
                query = query.Where(s => s.Name != null && s.Name.Contains(squadFilter.Name));
            }

            return await GetListAsync(query, squadFilter.Sort, squadFilter.SortDir, squadFilter.Page, squadFilter.PageSize).ConfigureAwait(false);
        }

        public async Task<bool> VerifySquadExistsAsync(int id)
        {
            return await Context.Squads.AnyAsync(s => s.Id == id).ConfigureAwait(false);
        }

        // Novo método para buscar um Squad com as aplicações relacionadas
        // SquadRepository.cs
        public async Task<Squad?> GetSquadWithApplicationsAsync(int id)
        {
            return await Context.Squads
                .Include(s => s.Applications) // Inclui as aplicações relacionadas
                .FirstOrDefaultAsync(s => s.Id == id)
                .ConfigureAwait(false);
        }


    }
}
