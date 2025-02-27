using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Domain.Entity;

namespace Stellantis.ProjectName.Application.Services
{
    public class SquadService
    {
        private readonly ISquadRepository _squadRepository;
        private readonly IStringLocalizer<ServiceResources> _localizer;

        public SquadService(ISquadRepository squadRepository, IStringLocalizer<ServiceResources> localizer)
        {
            _squadRepository = squadRepository;
            _localizer = localizer;
        }

        public void CreateSquad(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(_localizer["SquadNameRequired"]);
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException(_localizer["SquadDescriptionRequired"]);
            }

            var existingSquad = _squadRepository.GetByName(name);
            if (existingSquad != null)
            {
                throw new InvalidOperationException(_localizer["SquadNameAlreadyExists"]);
            }
            var squad = new EntitySquad
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description
            };
            _squadRepository.Add(squad);
        }

        public EntitySquad GetSquadById(Guid id)
        {
            var squad = _squadRepository.GetById(id);
            if (squad == null)
            {
                throw new KeyNotFoundException(_localizer["SquadNotFound"]);
            }
            return squad;
        }
    }
}
