
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;

namespace Stellantis.ProjectName.Application.Services
{
    public class IntegrationService : IEntityServiceBase<Integration>
    {
        private readonly IIntegrationRepository _integrationRepository;
        private readonly IStringLocalizer<ServiceResources> _localizer;

        public IntegrationService(IIntegrationRepository integrationRepository, IStringLocalizer<ServiceResources> localizer)
        {
            _integrationRepository = integrationRepository;
            _localizer = localizer;
        }

        public async Task<PagedResult<Integration>> GetListAsync(AreaFilter areaFilter)
        {
            ArgumentNullException.ThrowIfNull(areaFilter);

            var integrations = await _integrationRepository.GetListAsync(
                x => (string.IsNullOrEmpty(areaFilter.Name) || x.Name.Contains(areaFilter.Name)),
                areaFilter.Sort,
                areaFilter.SortDir,
                null,
                areaFilter.Page,
                areaFilter.PageSize
            ).ConfigureAwait(false);

            return integrations;
        }

        public async Task<OperationResult> CreateAsync(Integration item)
        {

            ArgumentNullException.ThrowIfNull(item);
            var existingIntegration = await _integrationRepository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingIntegration != null)
            {
                return OperationResult.Conflict(_localizer[ServiceResources.SquadNameAlreadyExists]);
            }

            await _integrationRepository.CreateAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(_localizer[ServiceResources.RegisteredSuccessfully]);
        }

        public async Task<OperationResult> DeleteAsync(int id)
        {
            var integration = await _integrationRepository.GetByIdAsync(id).ConfigureAwait(false);
            if (integration == null)
            {
                return OperationResult.NotFound(_localizer[ServiceResources.NotFound]);
            }

            await _integrationRepository.DeleteAsync(id).ConfigureAwait(false);
            return OperationResult.Complete(_localizer[ServiceResources.DeletedSuccessfully]);
        }

        public async Task<Integration?> GetItemAsync(int id)
        {
            return await _integrationRepository.GetByIdAsync(id).ConfigureAwait(false);
        }

        public async Task<OperationResult> UpdateAsync(Integration item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var existingIntegration = await _integrationRepository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingIntegration == null)
            {
                return OperationResult.NotFound(_localizer[ServiceResources.NotFound]);
            }

            await _integrationRepository.UpdateAsync(item).ConfigureAwait(false);
            return OperationResult.Complete(_localizer[ServiceResources.UpdatedSuccessfully]);
        }
    }
}
