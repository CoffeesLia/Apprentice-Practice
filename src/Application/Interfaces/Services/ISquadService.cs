using System;
using Stellantis.ProjectName.Domain.Entity;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface ISquadService
    {
        void CreateSquadAsync(string name, string description);
        Squad GetSquadById(Guid id);
        void UpdateSquad(Guid id, string name, string description);
        IEnumerable<EntitySquad> GetAllSquads(string name = null);
        void DeleteSquad(Guid id); 
    }

}

