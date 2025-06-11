using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class FeedbackService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory, IValidator<Feedback> validator)
            : EntityServiceBase<Feedback>(unitOfWork, localizerFactory, validator), IFeedbackService
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(FeedbackResources));
        protected override IFeedbackRepository Repository => UnitOfWork.FeedbackRepository;


        public override async Task<OperationResult> CreateAsync(Feedback item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Validação do objeto pelo FluentValidation
            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verificar se a aplicação existe
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false); 
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            // Validar se os membros estão nos squads da aplicação
            if (item.Members.Count > 0)
            {
                var validMemberIds = application.Squads
                    .SelectMany(s => s.Members)
                    .Select(m => m.Id)
                    .ToHashSet();

                var invalidMemberIds = item.Members
                    .Where(m => !validMemberIds.Contains(m.Id))
                    .Select(m => m.Id)
                    .ToList();

                if (invalidMemberIds.Count > 0)
                {
                    return OperationResult.Conflict(_localizer[nameof(FeedbackResources.InvalidMembers)]);
                }
            }

            item.CreatedAt = DateTime.UtcNow;
            if (item.FeedbackStatus == default)
            {
                item.FeedbackStatus = Status.Open;
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(Feedback item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Validação do objeto pelo FluentValidation
            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            // Verificar se o Feedbackse existe
            var existingFeedbacks = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingFeedbacks == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            // Verifica se a aplicação existe
            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false); 
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            // Valida membros se existirem
            if (item.Members.Count > 0)
            {
                var validMemberIds = application.Squads
                    .SelectMany(s => s.Members)
                    .Select(m => m.Id)
                    .ToHashSet();

                var invalidMemberIds = item.Members
                    .Where(m => !validMemberIds.Contains(m.Id))
                    .Select(m => m.Id)
                    .ToList();

                if (invalidMemberIds.Count > 0)
                {
                    return OperationResult.Conflict(_localizer[nameof(FeedbackResources.InvalidMembers)]);
                }

                existingFeedbacks.Members.Clear();
                foreach (var member in item.Members)
                {
                    existingFeedbacks.Members.Add(member);
                }
                existingFeedbacks.Members = item.Members;
            }

            // Atualiza dados básicos
            existingFeedbacks.Title = item.Title;
            existingFeedbacks.Description = item.Description;
            existingFeedbacks.ApplicationId = item.ApplicationId;

            // Controle de status e datas
            if (item.FeedbackStatus == Status.Closed && existingFeedbacks.ClosedAt == null)
            {
                existingFeedbacks.ClosedAt = DateTime.UtcNow;
            }
            else if (item.FeedbackStatus == Status.Reopened)
            {
                existingFeedbacks.ClosedAt = null;
            }

            existingFeedbacks.FeedbackStatus = item.FeedbackStatus;

            return await base.UpdateAsync(existingFeedbacks).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId)
        {
            return await Repository.GetMembersByApplicationIdAsync(applicationId);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var Feedback = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return Feedback != null
             ? OperationResult.Complete()
                : OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
        }

        public async Task<PagedResult<Feedback>> GetListAsync(FeedbackFilter filter)
        {
            filter ??= new FeedbackFilter();
            return await UnitOfWork.FeedbackRepository.GetListAsync(filter).ConfigureAwait(false);
        }

        public override async Task<OperationResult> DeleteAsync(int id)
        {
            var item = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }
            return await base.DeleteAsync(item).ConfigureAwait(false);
        }

    }
}
