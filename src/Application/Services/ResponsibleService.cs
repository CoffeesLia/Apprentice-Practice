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
                return OperationResult.Conflict(_localizer[nameof(ResponsibleResource.AlreadyExists)]);
            }

            // Verificação se o nome é obrigatório
            if (string.IsNullOrWhiteSpace(item.Nome))
            {
                return OperationResult.InvalidData(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure(nameof(item.Nome), _localizer["NameRequired"])
                }));
            }

            // Verificação se a área é obrigatória
            if (string.IsNullOrWhiteSpace(item.Area))
            {
                return OperationResult.InvalidData(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure(nameof(item.Area), _localizer["AreaRequired"])
                }));
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<Responsible>> GetListAsync(ResponsibleFilter responsibleFilter)
        {
            return await Repository.GetListAsync(responsibleFilter).ConfigureAwait(false);
        }
    }
}