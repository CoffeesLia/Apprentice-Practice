using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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

        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(ApplicationDataResources));

        
        protected override IApplicationDataRepository Repository =>
            UnitOfWork.ApplicationDataRepository;

        public override async Task<OperationResult> CreateAsync(ApplicationData item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (string.IsNullOrEmpty(item.Name))
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.NameRequired)]);
            }

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (!await IsApplicationNameUniqueAsync(item.Name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.AlreadyExists)]);
            }
            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {

            var applicationData = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (applicationData == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ApplicationDataResources.ApplicationNotFound)]);
            }
            var result = new
            {
                applicationData.Name,
                applicationData.Area
            };
            return OperationResult.Complete(result.ToString() ?? string.Empty);
        }

        public async Task<bool> IsApplicationNameUniqueAsync(string name, int? id = null)
        {
            var filter = new ApplicationFilter { Name = name };
            var applicationData = await GetListAsync(filter).ConfigureAwait(false);
            return !applicationData.Result.Any(a => a.Id != id); 
        }


        public override async Task<OperationResult> UpdateAsync(ApplicationData item)
        {
            ArgumentNullException.ThrowIfNull(item);
            if (string.IsNullOrEmpty(item.Name))
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.NameRequired)]);
            }

            if (!await IsApplicationNameUniqueAsync(item.Name).ConfigureAwait(false))
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.AlreadyExists)]);
            }
            return await base.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter)
        {
            applicationFilter ??= new ApplicationFilter();
            return await UnitOfWork.ApplicationDataRepository.GetListAsync(applicationFilter).ConfigureAwait(false);

        }

        public override async Task <OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetFullByIdAsync(id).ConfigureAwait(false);
            if (item == null)
                return OperationResult.NotFound(base.Localizer[nameof(ApplicationDataResources.ApplicationNotFound)]);
            return await base.DeleteAsync(item).ConfigureAwait(false);


        }
    }
}