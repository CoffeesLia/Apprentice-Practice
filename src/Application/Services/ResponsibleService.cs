using FluentValidation;
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
    public class ResponsibleService : EntityServiceBase<Responsible>, IResponsibleService
    {
        private readonly IStringLocalizer _localizer;
        protected override IResponsibleRepository Repository => UnitOfWork.ResponsibleRepository;

        public ResponsibleService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Responsible> validator)
            : base(unitOfWork, localizerFactory, validator)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            _localizer = localizerFactory.Create(typeof(ResponsibleResource));
        }

        public override async Task<OperationResult> CreateAsync(Responsible item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Validação do objeto pelo FluentValidation
            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verificação se o e-mail já existe
            if (await Repository.VerifyEmailAlreadyExistsAsync(item.Email).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ResponsibleResource.EmailExists)]);
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<Responsible>> GetListAsync(ResponsibleFilter responsibleFilter)
        {
            responsibleFilter ??= new ResponsibleFilter();
            return await UnitOfWork.ResponsibleRepository.GetListAsync(responsibleFilter).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var responsible = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return responsible != null
             ? OperationResult.Complete()
                : OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
        }

        public override async Task<OperationResult> UpdateAsync(Responsible item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Validação do objeto pelo FluentValidation
            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verificação se o e-mail já existe
            if (await Repository.VerifyEmailAlreadyExistsAsync(item.Email).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ResponsibleResource.EmailExists)]);
            }

            return await base.UpdateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ResponsibleResource.ResponsibleNotFound)]);
            }
            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

    }
}