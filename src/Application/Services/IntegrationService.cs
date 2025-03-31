using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Interfaces;
using FluentValidation;
using System.Linq.Expressions;
using FluentValidation.Results;

namespace Stellantis.ProjectName.Application.Services
{

    public class IntegrationService : EntityServiceBase<Integration>, IIntegrationService
    {
        private new IStringLocalizer Localizer => localizerFactory.Create(typeof(IntegrationResources));
        private readonly IStringLocalizerFactory localizerFactory;

        protected override IIntegrationRepository Repository => UnitOfWork.IntegrationRepository;

        public IntegrationService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Integration> validator)
            : base(unitOfWork, localizerFactory, validator)
        {
<<<<<<< HEAD
            this.localizerFactory = localizerFactory;
=======
            filter ??= new IntegrationFilter();
            return await Repository.GetListAsync(filter).ConfigureAwait(false);
>>>>>>> 8349a696a81a49b6a6a2fbfeac69af4ca38aed2d
        }

        public async Task<PagedResult<Integration>> GetListAsync(IntegrationFilter filter)
        {
            filter ??= new IntegrationFilter();
            return await Repository.GetListAsync(filter).ConfigureAwait(false);
        }
        public async Task<OperationResult> CreateAsync(Integration item)
        {
            ArgumentNullException.ThrowIfNull(item);
<<<<<<< HEAD

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }
=======
>>>>>>> 8349a696a81a49b6a6a2fbfeac69af4ca38aed2d

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }
            await base.CreateAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[IntegrationResources.MessageSucess]);
        }

        public new async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
            { return OperationResult.NotFound(Localizer[IntegrationResources.MessageNotFound]); }
            return await DeleteAsync(item).ConfigureAwait(false);
        }

        public new async Task<Integration?> GetItemAsync(int id)
        {
            var integration = await Repository.GetByIdAsync(id).ConfigureAwait(false);
<<<<<<< HEAD
            return integration ?? throw new KeyNotFoundException(Localizer[IntegrationResources.MessageNotFound]);
=======
            return integration ?? throw new InvalidOperationException(Localizer[IntegrationResources.MessageNotFound]);
>>>>>>> 8349a696a81a49b6a6a2fbfeac69af4ca38aed2d
        }

        public override async Task<OperationResult> UpdateAsync(Integration item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
<<<<<<< HEAD
            {
                return OperationResult.InvalidData(validationResult);
            }
            var existingIntegration = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingIntegration == null)
            {
                return OperationResult.NotFound(Localizer[IntegrationResources.MessageNotFound]);
=======
            {
                return OperationResult.InvalidData(validationResult);
>>>>>>> 8349a696a81a49b6a6a2fbfeac69af4ca38aed2d
            }
            await Repository.UpdateAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[IntegrationResources.MessageSucess]);
        }
    }
}