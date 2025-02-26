using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class SquadService
    {
       private readonly ISquadRepository _squadRepository;

        public SquadService(ISquadRepository squadRepository)
        {
            _squadRepository = squadRepository;
        }

        public void CreateSquad(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Squad name is required.");
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Squad description is required.");
            }

            var existingSquad = _squadRepository.GetByName(name);
            if (existingSquad != null)
            {
                throw new InvalidOperationException("A squad with this name already exists.");
            }
            var squad = new EntitieSquad
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description
            };
            _squadRepository.Add(squad);
        }

    }
}
