using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Domain.Entity;
using Stellantis.ProjectName.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stellantis.ProjectName.Infrastructure.Repositories
{
    public class SquadRepository : ISquadRepository
    {
        private readonly Context _context;

        public SquadRepository(Context context)
        {
            _context = context;
        }

        public void Add(EntitySquad squad)
        {
            _context.Squads.Add(squad);
            _context.SaveChanges();
        }

        public EntitySquad GetByName(string name)
        {
            return _context.Squads.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<EntitySquad> GetAll()
        {
            return _context.Squads.ToList();
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
