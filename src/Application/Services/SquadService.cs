using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class SquadService(ISquadRepository squadRepository, IStringLocalizer<SquadResources> localizer, IValidator<Squad> validator) : ISquadService
    {
        private readonly ISquadRepository _squadRepository = squadRepository ?? throw new ArgumentNullException(nameof(squadRepository));
        private readonly IStringLocalizer<SquadResources> _localizer = localizer;
        private readonly IValidator<Squad> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        public async Task<Squad?> GetItemAsync(int id)
        {
            var squad = await _squadRepository.GetByIdAsync(id).ConfigureAwait(false);
            return squad ?? throw new KeyNotFoundException(_localizer[nameof(SquadResources.SquadNotFound)]);
        }

        public async Task<PagedResult<Squad>> GetListAsync(SquadFilter squadFilter)
        {
            squadFilter ??= new SquadFilter { Name = string.Empty };
            var result = await _squadRepository.GetListAsync(squadFilter).ConfigureAwait(false);
            if (result == null || !result.Result.Any())
            {
                throw new KeyNotFoundException(_localizer[nameof(SquadResources.SquadNotFound)]);
            }
            return result;
        }

        public async Task<OperationResult> CreateAsync(Squad squad)
        {
            if (squad == null)
            {
                throw new ArgumentNullException(nameof(squad), _localizer[nameof(SquadResources.SquadCannotBeNull)]);
            }

            if (squad.Name?.Length > 50 || squad.Name?.Length < 3)
            {
                throw new ArgumentException(_localizer[nameof(SquadResources.NameValidateLength)]);
            }

            var validationResult = await _validator.ValidateAsync(squad).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }
            if (await _squadRepository.VerifyNameAlreadyExistsAsync(squad.Name ?? string.Empty).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(SquadResources.SquadNameAlreadyExists)]);
            }
            await _squadRepository.CreateAsync(squad).ConfigureAwait(false);
            return OperationResult.Complete();
        }

        public async Task<OperationResult> UpdateAsync(Squad squad)
        {
            await _squadRepository.UpdateAsync(squad).ConfigureAwait(false);
            return OperationResult.Complete();
        }

        public async Task<OperationResult> DeleteAsync(int id)
        {
            if (!await _squadRepository.VerifySquadExistsAsync(id).ConfigureAwait(false))
            {
                return OperationResult.NotFound(_localizer[nameof(SquadResources.SquadNotFound)]);
            }
            await _squadRepository.DeleteAsync(id).ConfigureAwait(false);
            return OperationResult.Complete();
        }

        public async Task<bool> VerifySquadExistsAsync(int id)
        {
            if (await _squadRepository.VerifySquadExistsAsync(id).ConfigureAwait(false))
            {
                throw new ArgumentException(_localizer[nameof(SquadResources.SquadNameAlreadyExists)]);
            }
            return false;
        }

        public async Task<bool> VerifyNameAlreadyExistsAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(_localizer[nameof(SquadResources.SquadCannotBeNull)]);
            }

            if (await _squadRepository.VerifyNameAlreadyExistsAsync(name).ConfigureAwait(false))
            {
                throw new ArgumentException(_localizer[nameof(SquadResources.SquadNameAlreadyExists)]);
            }
            return false;
        }

    }
}