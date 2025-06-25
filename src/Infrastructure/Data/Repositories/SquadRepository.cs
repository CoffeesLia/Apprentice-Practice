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

        public async Task<PagedResult<Squad>> GetListAsync(SquadFilter squadFilter)
        {
            ArgumentNullException.ThrowIfNull(squadFilter);

            squadFilter.Page = squadFilter.Page <= 0 ? 1 : squadFilter.Page;

            // Inclui os membros no carregamento
            IQueryable<Squad> query = Context.Squads
                .Include(s => s.Members);

            if (!string.IsNullOrEmpty(squadFilter.Name))
            {
                query = query.Where(s => s.Name != null && s.Name.Contains(squadFilter.Name));
            }

            var pagedResult = await GetListAsync(query, squadFilter.Sort, squadFilter.SortDir, squadFilter.Page, squadFilter.PageSize).ConfigureAwait(false);

            // Preenche o custo total em cada squad (se existir a propriedade Cost)
            foreach (var squad in pagedResult.Result)
            {
                if (squad.Members != null)
                    squad.Cost = squad.Members.Sum(m => m.Cost);
            }

            return pagedResult;
        }

        public async Task<bool> VerifyNameAlreadyExistsAsync(string name)
        {
            return await Context.Squads.AnyAsync(s => s.Name == name).ConfigureAwait(false);
        }

        public async Task<bool> VerifySquadExistsAsync(int id)
        {
            return await Context.Squads.AnyAsync(s => s.Id == id).ConfigureAwait(false);
        }
    }
}