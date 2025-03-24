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

            // Validação do objeto Responsible
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

            // Verificação se o nome é obrigatório
            if (string.IsNullOrWhiteSpace(item.Nome))
            {
                return OperationResult.InvalidData(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure(nameof(item.Nome), _localizer[nameof(ResponsibleResource.NameRequired)])
                }));
            }

            // Verificação se a área é obrigatória
            if (string.IsNullOrWhiteSpace(item.Area))
            {
                return OperationResult.InvalidData(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure(nameof(item.Area), _localizer[nameof(ResponsibleResource.AreaRequired)])
                }));
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
            return await Repository.GetByIdAsync(id).ConfigureAwait(false) is Responsible responsible
                ? OperationResult.Complete()
                : OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
        }

        public override async Task<OperationResult> UpdateAsync(Responsible item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Validação do objeto Responsible
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

            // Verificação se o nome é obrigatório
            if (string.IsNullOrWhiteSpace(item.Nome))
            {
                return OperationResult.InvalidData(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure(nameof(item.Nome), _localizer[nameof(ResponsibleResource.NameRequired)])
                }));
            }

            // Verificação se a área é obrigatória
            if (string.IsNullOrWhiteSpace(item.Area))
            {
                return OperationResult.InvalidData(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure(nameof(item.Area), _localizer[nameof(ResponsibleResource.AreaRequired)])
                }));
            }

            return await base.UpdateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
            {
                return OperationResult.NotFound(_localizer[nameof(OperationResult.NotFound)]);
            }
            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

    }
}