using System;
using Stellantis.ProjectName.Domain.Entity;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface ISquadService
    {
        void CreateSquad(string name, string description);
        EntitySquad GetSquadById(Guid id);
        void UpdateSquad(Guid id, string name, string description);
    }
}

