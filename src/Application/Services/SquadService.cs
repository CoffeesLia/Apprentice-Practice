using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Domain.Entity;

namespace Stellantis.ProjectName.Application.Services
{
    public class SquadService : ISquadService
    {
        private readonly ISquadRepository _squadRepository;
        private readonly IStringLocalizer<SquadResources> _localizer;

        public SquadService(ISquadRepository squadRepository, IStringLocalizer<SquadResources> localizer)
        {
            _squadRepository = squadRepository;
            _localizer = localizer;
        }

        public void CreateSquad(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(_localizer[nameof(SquadResources.SquadNameRequired)]);
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException(_localizer[nameof(SquadResources.SquadDescriptionRequired)]);
            }

            var existingSquad = _squadRepository.GetByName(name);
            if (existingSquad != null)
            {
                throw new InvalidOperationException(_localizer[nameof(SquadResources.SquadNameAlreadyExists)]);
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
                throw new KeyNotFoundException(_localizer[nameof(SquadResources.SquadNotFound)]);
            }
            return squad;
        }

        public void UpdateSquad(Guid id, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(_localizer[nameof(SquadResources.SquadNameRequired)]);
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException(_localizer[nameof(SquadResources.SquadDescriptionRequired)]);
            }

            var squad = _squadRepository.GetById(id);
            if (squad == null)
            {
                throw new KeyNotFoundException(_localizer[nameof(SquadResources.SquadNotFound)]);
            }

            var existingSquad = _squadRepository.GetByName(name);
            if (existingSquad != null && existingSquad.Id != id)
            {
                throw new InvalidOperationException(_localizer[nameof(SquadResources.SquadNameAlreadyExists)]);
            }

            squad.Name = name;
            squad.Description = description;
            _squadRepository.Update(squad);
        }

        public IEnumerable<EntitySquad> GetAllSquads(string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return _squadRepository.GetAll();
            }
            await _squadRepository.DeleteAsync(id).ConfigureAwait(false);
        }

    }
}
