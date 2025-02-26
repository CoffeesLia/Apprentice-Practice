using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class ApplicationDataService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<ApplicationData> validator)
        : EntityServiceBase<ApplicationData>(unitOfWork, localizerFactory, validator), IApplicationDataService
    {
        private IStringLocalizer localizer => localizerFactory.Create(typeof(ApplicationDataResources));

        protected override IApplicationDataRepository Repository =>
            UnitOfWork.ApplicationDataRepository;

        public async Task<bool> ApplicationDataUniqueNameAsync(string nameApplication, int? id = null)
        {
            var filter = new ApplicationFilter { NameApplication = nameApplication };
            var applicationData = await GetListAsync(filter).ConfigureAwait(false);
            return !applicationData.Result.Any(a => a.Id != id);
        }

        public Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter)
        {
            throw new NotImplementedException();
        }

        public override async Task<OperationResult> CreateAsync(ApplicationData item)
        {
            ArgumentNullException.ThrowIfNull(item);


            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }
            if(await ApplicationDataUniqueNameAsync(item.NameApplication).ConfigureAwait(false))
            {
                return OperationResult.Conflict(localizer[nameof(ApplicationDataResources.AlreadyExists)]);
            }

            var area = await UnitOfWork.AreaRepository.GetByIdAsync(item.AreaId).ConfigureAwait(false);
         
            item.Area = area;

            return await base.CreateAsync(item).ConfigureAwait(false);


        }
    }
}
