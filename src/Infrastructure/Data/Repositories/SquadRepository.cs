// Stellantis.ProjectName.Infrastructure.Data.Repositories/SquadRepository.cs
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Models; // Certifique-se de que este using existe para PagedResult

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class SquadRepository(Context context) : RepositoryEntityBase<Squad, Context>(context), ISquadRepository
    {
        // Se você está usando métodos base (CreateAsync, GetByIdAsync, UpdateAsync, DeleteAsync)
        // do seu RepositoryEntityBase, você pode não precisar sobrescrevê-los aqui
        // a menos que precise de uma lógica específica para Squad.
        // Mantenho como estava, mas saiba que se o base já faz o que precisa, pode simplificar.

        public new async Task CreateAsync(Squad squad, bool saveChanges = true)
        {
            await base.CreateAsync(squad, saveChanges).ConfigureAwait(false);
        }

        // Importante: Este GetByIdAsync base não inclui as aplicações.
        // O GetSquadWithApplicationsAsync é o que você usará para buscar com as aplicações.
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

        // *** ALTERAÇÃO CRUCIAL AQUI: INCLUINDO AS APLICAÇÕES NA LISTAGEM ***
        public async Task<PagedResult<Squad>> GetListAsync(SquadFilter squadFilter)
        {
            ArgumentNullException.ThrowIfNull(squadFilter);

            // Garante que o valor de Page seja pelo menos 1
            squadFilter.Page = squadFilter.Page <= 0 ? 1 : squadFilter.Page;

            // Inclui as aplicações no Query antes de aplicar filtros e paginação
            IQueryable<Squad> query = Context.Squads
                                            .Include(s => s.Applications) // <-- ADICIONADO: Carrega as aplicações
                                            .AsQueryable();

            if (!string.IsNullOrEmpty(squadFilter.Name))
            {
                query = query.Where(s => s.Name != null && s.Name.Contains(squadFilter.Name));
            }

            // O método base GetListAsync que você está chamando provavelmente já faz a paginação, ordenação, etc.
            return await GetListAsync(query, squadFilter.Sort, squadFilter.SortDir, squadFilter.Page, squadFilter.PageSize).ConfigureAwait(false);
        }

        public async Task<bool> VerifySquadExistsAsync(int id)
        {
            return await Context.Squads.AnyAsync(s => s.Id == id).ConfigureAwait(false);
        }

        // Este método já estava correto para buscar um Squad com as aplicações relacionadas
        public async Task<Squad?> GetSquadWithApplicationsAsync(int id)
        {
            return await Context.Squads
                .Include(s => s.Applications) // Inclui as aplicações relacionadas
                .FirstOrDefaultAsync(s => s.Id == id)
                .ConfigureAwait(false);
        }

        // *** NOVO MÉTODO: Implementação para buscar um squad pelo nome ***
        public async Task<Squad?> GetByNameAsync(string name)
        {
            return await Context.Squads
                .FirstOrDefaultAsync(s => s.Name == name)
                .ConfigureAwait(false);
        }
    }
}