using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Domain.Entity;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface ISquadRepository
    {
        void Add(EntitySquad squad);
        EntitySquad GetByName(String name);
        IEnumerable<EntitySquad> GetAll();
        EntitySquad GetById(Guid id); 
    }
}
