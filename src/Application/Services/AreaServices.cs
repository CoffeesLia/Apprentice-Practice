using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.Application.Services
{
    public class AreaService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Area> validator)
        : EntityServiceBase<Area>(unitOfWork, localizerFactory, validator), IAreaService
    {
        private IStringLocalizer _localizer => localizerFactory.Create(typeof(AreaResources));
        /*
O nome da área deve ter um número mínimo de 3 e máximo de 255 de caracteres definidos.

O sistema deve confirmar o sucesso da criação da área.O sistema deve permitir a edição do nome da área.


Pendências:
        */
        protected override IAreaRepository Repository => UnitOfWork.AreaRepository;

        public override async Task<OperationResult> CreateAsync(Area item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }
            if (await Repository.VerifyNameAlreadyExistsAsync(item.Name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(AreaResources.AlreadyExists)]);
            }
            return await base.CreateAsync(item).ConfigureAwait(false);
        }

    }
}