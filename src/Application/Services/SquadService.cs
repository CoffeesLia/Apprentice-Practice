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

            // *** ALTERAÇÃO AQUI: Lógica aprimorada para verificar nome existente em Update ***
            if (await Repository.VerifyNameAlreadyExistsAsync(squad.Name!).ConfigureAwait(false))
            {
                // Se o nome já existe, agora precisamos verificar se pertence ao próprio squad que está sendo atualizado.
                var squadWithSameName = await Repository.GetByNameAsync(squad.Name!).ConfigureAwait(false);
                if (squadWithSameName != null && squadWithSameName.Id != squad.Id) // Se o nome pertence a OUTRO squad
                {
                    return OperationResult.Conflict(_localizer[nameof(SquadResources.SquadNameAlreadyExists)]);
                }
            }
            // Se o nome não existe, ou se o nome existe mas é do próprio squad, prossegue com a atualização.

            // Atualize as propriedades do existingSquad com os novos valores
            existingSquad.Name = squad.Name;
            existingSquad.Description = squad.Description;
            // Se houver outras propriedades (exceto coleções que são tratadas separadamente), atualize-as aqui.

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
            // A chamada para o repositório já está OK aqui, pois o SquadRepository.GetListAsync foi corrigido para incluir as Applications.
            return await Repository.GetListAsync(squadFilter).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var squad = await Repository.GetSquadWithApplicationsAsync(id).ConfigureAwait(false);
            if (squad == null)
            {
                return OperationResult.NotFound(_localizer[nameof(SquadResources.SquadNotFound)]);
            }
            return OperationResult.Complete();
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

        public async Task<OperationResult> AddApplicationsToSquadAsync(int squadId, Collection<int> applicationIds)
        {
            if (applicationIds == null || applicationIds.Count == 0)
            {
                return OperationResult.InvalidData(new ValidationResult(
                [
                    new ValidationFailure("ApplicationIds",
                _localizer[nameof(SquadResources.ApplicationIdsCannotBeEmpty)].Value)
                ]));
            }

            var squad = await Repository.GetSquadWithApplicationsAsync(squadId).ConfigureAwait(false);
            if (squad == null)
            {
                return OperationResult.NotFound(_localizer[nameof(SquadResources.SquadNotFound)]);
            }

            var applications = await UnitOfWork.ApplicationDataRepository
                .GetListAsync(a => applicationIds.Contains(a.Id))
                .ConfigureAwait(false);

            if (applications == null || applications.Count == 0)
            {
                return OperationResult.NotFound(_localizer[nameof(SquadResources.ApplicationsNotFound)]);
            }

            foreach (var application in applications)
            {
                if (!squad.Applications.Any(a => a.Id == application.Id))
                {
                    squad.Applications.Add(application);
                }
            }

            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            return OperationResult.Complete(_localizer[nameof(SquadResources.ApplicationsLinkedSuccessfully)]);
        }
    }
}