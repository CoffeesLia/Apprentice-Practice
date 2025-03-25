using System;
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

namespace Stellantis.ProjectName.Application.Services
{

    public class IntegrationService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Integration> validator)
        : EntityServiceBase<Integration>(unitOfWork, localizerFactory, validator), IIntegrationService
    {
        private new IStringLocalizer Localizer => localizerFactory.Create(typeof(IntegrationResources));
        private readonly IStringLocalizerFactory localizerFactory = localizerFactory;

        protected override IIntegrationRepository Repository => UnitOfWork.IntegrationRepository;

        public async Task<PagedResult<Integration>> GetListAsync(IntegrationFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);
            Expression<Func<Integration, bool>>? filterExpression = null;

            if (!string.IsNullOrEmpty(filter.Name))
            {
                OperationResult.Conflict(Localizer[IntegrationResources.NameIsRequired]);
            }

            return await Repository.GetListAsync(filterExpression, filter.Sort, filter.SortDir, null, filter.Page, filter.PageSize).ConfigureAwait(false);
        }

        public override async Task<OperationResult> CreateAsync(Integration item)
        {
            ArgumentNullException.ThrowIfNull(item);
          
            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            await Repository.CreateAsync(item).ConfigureAwait(true);
            return OperationResult.Complete(Localizer[IntegrationResources.MessageSucess]);
        }

        public new async Task<OperationResult> DeleteAsync(int id)
        {
            var integration = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (integration == null)
            {
                return OperationResult.NotFound(Localizer[IntegrationResources.MessageNotFound]);
            }

            await Repository.DeleteAsync(id).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[IntegrationResources.DeletedSuccessfully]);
        }

        public new async Task<Integration?> GetItemAsync(int id)
        {
            var integration = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (integration == null)
            {
                throw new KeyNotFoundException(Localizer[IntegrationResources.MessageNotFound]);
            }
            return integration;
        }

        public override async Task<OperationResult> UpdateAsync(Integration item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var existingIntegration = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingIntegration == null)
            {
                return OperationResult.NotFound(Localizer[IntegrationResources.NameIsRequired]);
            }

            await Repository.UpdateAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(Localizer[IntegrationResources.UpdatedSuccessfully]);
        }

        public Task<bool> IsIntegrationNameUniqueAsync(string name, int? id = null)
        {
            throw new NotImplementedException();
        }
    }
}