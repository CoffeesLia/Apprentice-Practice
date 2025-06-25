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

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            if (item.Members != null && item.Members.Count > 0)
            {
                var validMemberIds = application?.Squads?.Members?.Select(m => m.Id).ToHashSet() ?? [];
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
            if (item.Status == default)
            {
                item.Status = FeedbackStatus.Open;
            }

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(Feedback item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var validationResult = await Validator.ValidateAsync(item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            var existingFeedback = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingFeedback == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            if (item.Members.Count > 0)
            {
                var validMemberIds = application.Squads.Members
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

                existingFeedback.Members = item.Members;
            }

            existingFeedback.Title = item.Title;
            existingFeedback.Description = item.Description;
            existingFeedback.ApplicationId = item.ApplicationId;

            if (item.Status == FeedbackStatus.Closed && existingFeedback.ClosedAt == null)
            {
                existingFeedback.ClosedAt = DateTime.UtcNow;
            }
            else if (item.Status == FeedbackStatus.Reopened)
            {
                existingFeedback.ClosedAt = null;
            }

            existingFeedback.Status = item.Status;

            return await base.UpdateAsync(existingFeedback).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId)
        {
            return await Repository.GetMembersByApplicationIdAsync(applicationId).ConfigureAwait(false);
        }

        public new async Task<OperationResult> GetItemAsync(int id)
        {
            var feedback = await Repository.GetByIdAsync(id).ConfigureAwait(false);
            return feedback != null
             ? OperationResult.Complete()
                : OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
        }

        public async Task<PagedResult<Feedback>> GetListAsync(FeedbackFilter feedbackFilter)
        {
            feedbackFilter ??= new FeedbackFilter();
            return await UnitOfWork.FeedbackRepository.GetListAsync(feedbackFilter).ConfigureAwait(false);
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
