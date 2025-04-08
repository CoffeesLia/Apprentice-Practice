using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Domain.Entity;
using Stellantis.ProjectName.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stellantis.ProjectName.Infrastructure.Repositories
{
    public class SquadRepository : RepositoryEntityBase<Squad, Context>, ISquadRepository
    {
        private readonly Context _context;

        public SquadRepository(Context context) : base(context)
        {
            _context = context;
        }

        public SquadRepository(Context context)
        {
            _context = context;
        }

        public async Task<Squad?> GetByIdAsync(int id)
        {
            _context.Squads.Add(squad);
            _context.SaveChanges();
        }

        public EntitySquad GetByName(string name)
        {
            _context.Squads.Update(squad);

            if (saveChanges)
            {
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public IEnumerable<EntitySquad> GetAll()
        {
            var squad = await _context.Squads.FindAsync(id).ConfigureAwait(false);
            if (squad != null)
            {
                _context.Squads.Remove(squad);

                if (saveChanges)
                {
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<bool> VerifyNameAlreadyExistsAsync(string name)
        {
            return await _context.Squads.AnyAsync(s => s.Name == name).ConfigureAwait(false);
        }

        public EntitySquad GetById(Guid id)
        {
            return _context.Squads.FirstOrDefault(s => s.Id == id);
        }

        public void Update(EntitySquad squad)
        {
            _context.Squads.Update(squad);
            _context.SaveChanges();
        }
    }

}
