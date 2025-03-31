using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Domain.Entity;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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

        public void Add(Squad squad)
        {
            _context.Squads.Add(squad);
        }

        public Squad GetByName(string name)
        {
            return _context.Squads.FirstOrDefault(s => s.Name == name);
        }

        public IEnumerable<Squad> GetAll()
        {
            return _context.Squads.ToList();
        }

        public Squad GetById(Guid id)
        {
            return _context.Squads.Find(id);
        }

        public void Update(Squad squad)
        {
            _context.Squads.Update(squad);
        }

        public void Delete(Squad squad)
        {
            _context.Squads.Remove(squad);
        }

        public void Delete(EntitySquad squad)
        {
            throw new NotImplementedException();
        }
    }
}
