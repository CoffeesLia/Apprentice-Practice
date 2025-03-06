using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class ApplicationDataService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<ApplicationData> validator)
        : EntityServiceBase<ApplicationData>(unitOfWork, localizerFactory, validator), IApplicationDataService
    {
        private IStringLocalizer localizer => localizerFactory.Create(typeof(ApplicationDataResources));

        protected override IApplicationDataRepository Repository =>
            UnitOfWork.ApplicationDataRepository;

        public async Task<bool> IsAreaNameUniqueAsync(string name, int? id = null)
        {
            var filter = new ApplicationFilter { Name = name};
            var applicationData = await GetListAsync(filter).ConfigureAwait(false);
            return !applicationData.Result.Any(a => a.Id != id);
        }

        public async Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter)
        {
            return await Repository.GetListAsync(applicationFilter).ConfigureAwait(false);
        }

        public override async Task<OperationResult> CreateAsync(ApplicationData item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }
            if (string.IsNullOrEmpty(item.Name))
            {
                return OperationResult.Conflict(localizer[nameof(ApplicationDataResources.NameRequired)]);
            }
            if (!await IsAreaNameUniqueAsync(item.Name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(localizer[nameof(ApplicationDataResources.AlreadyExists)]);
            }
            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var applicationData = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (applicationData == null)
            {
                return OperationResult.NotFound(localizer[nameof(ApplicationDataResources.ApplicationNotFound)]);
            }
            var result = new
            {
                applicationData.Name,
                applicationData.Area
            };
            return OperationResult.Complete(result.ToString());
        }

    }
}