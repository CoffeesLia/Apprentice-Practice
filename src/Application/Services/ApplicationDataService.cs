using System.Globalization;
using System.Text;
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
    public class ApplicationDataService : EntityServiceBase<ApplicationData>, IApplicationDataService
    {
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationExportService _exportService;

        public ApplicationDataService(
            IUnitOfWork unitOfWork,
            IStringLocalizerFactory localizerFactory,
            IValidator<ApplicationData> validator)
            : base(unitOfWork, localizerFactory, validator)
        {
            _localizer = localizerFactory.Create(typeof(ApplicationDataResources));
            _exportService = new ApplicationExportService(unitOfWork, localizerFactory);
        }

        protected override IApplicationDataRepository Repository =>
            UnitOfWork.ApplicationDataRepository;
        public override async Task<OperationResult> CreateAsync(ApplicationData item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(item.Name);

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

        public override async Task<OperationResult> UpdateAsync(ApplicationData item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(item.Name);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            if (!await IsApplicationNameUniqueAsync(item.Name, item.Id).ConfigureAwait(false))
            {
                return OperationResult.Conflict(ApplicationDataResources.AlreadyExists);
            }


            return await base.UpdateAsync(item).ConfigureAwait(false);
        }
        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetFullByIdAsync(id).ConfigureAwait(false);
            if (item == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ApplicationDataResources.ApplicationNotFound)]);
            }

            // Verifica integrações vinculadas
            var integrations = await UnitOfWork.IntegrationRepository.GetListAsync(new IntegrationFilter { ApplicationDataId = id }).ConfigureAwait(false);
            if (integrations.Result.Any())
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.IntegrationLinkedError)]);
            }

            // Verifica serviços vinculados
            var services = await UnitOfWork.ServiceDataRepository.GetListAsync(new ServiceDataFilter { ApplicationId = id }).ConfigureAwait(false);
            if (services.Result.Any())
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.ServiceLinkedError)]);
            }

            // Verifica repositórios vinculados
            var repos = await UnitOfWork.RepoRepository.GetListAsync(new RepoFilter { ApplicationId = id }).ConfigureAwait(false);
            if (repos.Result.Any())
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.RepoLinkedError)]);
            }

            // Verifica documentos vinculados
            var documents = await UnitOfWork.DocumentDataRepository.GetListAsync(new DocumentDataFilter { ApplicationId = id }).ConfigureAwait(false);
            if (documents.Result.Any())
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.DocumentLinkedError)]);
            }

            // Verifica conhecimentos vinculados
            var knowledges = await UnitOfWork.KnowledgeRepository.GetListAsync(new KnowledgeFilter { ApplicationId = id }).ConfigureAwait(false);
            if (knowledges.Result.Any())
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.KnowledgeLinkedError)]);
            }

            // Verifica feedbacks vinculados
            var feedbacks = await UnitOfWork.FeedbackRepository.GetMembersByApplicationIdAsync(id).ConfigureAwait(false);
            if (feedbacks.Any())
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.FeedbackLinkedError)]);
            }

            // Verifica incidentes vinculados
            var incidents = await UnitOfWork.IncidentRepository.GetMembersByApplicationIdAsync(id).ConfigureAwait(false);
            if (incidents.Any())
            {
                return OperationResult.Conflict(_localizer[nameof(ApplicationDataResources.IncidentLinkedError)]);
            }

            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

        public async Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter)
        {
            applicationFilter ??= new ApplicationFilter();

            return await UnitOfWork.ApplicationDataRepository.GetListAsync(applicationFilter).ConfigureAwait(false);
        }

        public async Task<bool> IsApplicationNameUniqueAsync(string name, int? id = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var existingItems = await Repository.GetListAsync(new ApplicationFilter { Name = name }).ConfigureAwait(false);
            if (existingItems?.Result == null)
            {
                return true;
            }

            // Comparação exata, ignorando maiúsculas/minúsculas
            return !existingItems.Result.Any(e =>
                e.Id != id &&
                string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase)
            );
        }

        public async Task<bool> IsResponsibleFromArea(int areaId, int responsibleId)
        {
            var responsible = await UnitOfWork.ResponsibleRepository.GetByIdAsync(responsibleId).ConfigureAwait(false);
            if (responsible == null)
            {
                return false;
            }
            return responsible.AreaId == areaId;
        }

        public async Task<byte[]> ExportToCsvAsync(ApplicationFilter filter)
        {
            return await _exportService.ExportToCsvAsync(filter);
        }

        public async Task<byte[]> ExportToPdfAsync(ApplicationFilter filter)
        {
            return await _exportService.ExportToPdfAsync(filter);
        }
    }
}