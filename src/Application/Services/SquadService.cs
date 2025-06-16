using System.Collections.ObjectModel;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class SquadService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Squad> validator)
        : EntityServiceBase<Squad>(unitOfWork, localizerFactory, validator), ISquadService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(SquadResources));
        protected override ISquadRepository Repository => UnitOfWork.SquadRepository;

        public override async Task<OperationResult> CreateAsync(Squad squad)
        {
            if (squad == null)
            {
                return OperationResult.Conflict(_localizer[nameof(SquadResources.SquadCannotBeNull)]);
            }

            var validationResult = await Validator.ValidateAsync(squad).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (await Repository.VerifyNameAlreadyExistsAsync(squad.Name!).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(SquadResources.SquadNameAlreadyExists)]);
            }

            return await base.CreateAsync(squad).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(Squad squad)
        {
            if (squad == null)
            {
                return OperationResult.Conflict(_localizer[nameof(SquadResources.SquadCannotBeNull)]);
            }

            var validationResult = await Validator.ValidateAsync(squad).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            var existingSquad = await Repository.GetByIdAsync(squad.Id).ConfigureAwait(false);
            if (existingSquad == null)
            {
                return OperationResult.NotFound(_localizer[nameof(SquadResources.SquadNotFound)]);
            }

            if (await Repository.VerifyNameAlreadyExistsAsync(squad.Name!).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(SquadResources.SquadNameAlreadyExists)]);
            }

            return await base.UpdateAsync(existingSquad).ConfigureAwait(false); // Atualiza o objeto existente
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            if (!await Repository.VerifySquadExistsAsync(id).ConfigureAwait(false))
            {
                return OperationResult.NotFound(_localizer[nameof(SquadResources.SquadNotFound)]);
            }

            await Repository.DeleteAsync(id, true).ConfigureAwait(false);
            return OperationResult.Complete();
        }

        public async Task<PagedResult<Squad>> GetListAsync(SquadFilter squadFilter)
        {
            squadFilter ??= new SquadFilter();
            return await Repository.GetListAsync(squadFilter).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var squad = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return squad != null
               ? OperationResult.Complete()
               : OperationResult.NotFound(_localizer[nameof(SquadResources.SquadNotFound)]);
        }

        public async Task<OperationResult> VerifySquadExistsAsync(int id)
        {
            if (await Repository.VerifySquadExistsAsync(id).ConfigureAwait(false))
            {
                return OperationResult.Complete();
            }
            return OperationResult.NotFound(_localizer[nameof(SquadResources.SquadNotFound)]);
        }

        public async Task<OperationResult> VerifyNameAlreadyExistsAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return OperationResult.Conflict(_localizer[nameof(SquadResources.SquadCannotBeNull)]);
            }

            if (await Repository.VerifyNameAlreadyExistsAsync(name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(SquadResources.SquadNameAlreadyExists)]);
            }
            return OperationResult.Complete();
        }
    }
}