using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Domain.Entity;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface ISquadRepository
    {
        void Add(EntitySquad squad);
        EntitySquad GetByName(string name);
        IEnumerable<EntitySquad> GetAll();
        EntitySquad GetById(Guid id);
        void Update(EntitySquad squad);
        void Delete(EntitySquad squad); 
    }
}
