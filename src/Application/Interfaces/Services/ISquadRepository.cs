using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface ISquadRepository
    {
        void Add(EntitieSquad squad);
        EntitieSquad GetByName(String name);
        IEnumerable<EntitieSquad> GetAll();
    }
}
