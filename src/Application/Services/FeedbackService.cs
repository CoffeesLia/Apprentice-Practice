using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Services;
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

            // Se vier apenas os IDs dos membros (ex: [{ Id = 1 }, { Id = 2 }]), busque os membros completos
            if (item.Members != null && item.Members.Count > 0)
            {
                var memberIds = item.Members.Select(m => m.Id).ToList();
                var pagedMembers = await UnitOfWork.MemberRepository
                    .GetListAsync(m => memberIds.Contains(m.Id)).ConfigureAwait(false);
                item.Members = pagedMembers.Result.ToList();
            }
            else
            {
                item.Members = new List<Member>();
            }

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

            // Validar se os membros estão no squad da aplicação
            if (item.Members != null && item.Members.Count > 0)
            {
                var squadId = application.SquadId;
                var invalidMemberIds = item.Members
                    .Where(m => m.SquadId != squadId)
                    .Select(m => m.Id)
                    .ToList();

                if (invalidMemberIds.Count > 0)
                {
                    return OperationResult.Conflict(_localizer[nameof(FeedbackResources.InvalidMembers)]);
                }
            }

            item.CreatedAt = DateTime.UtcNow;

            return await base.CreateAsync(item).ConfigureAwait(false);
        }

        public override async Task<OperationResult> UpdateAsync(Feedback item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var existingFeedback = await Repository.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existingFeedback == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            existingFeedback.Title = item.Title;
            existingFeedback.Description = item.Description;
            existingFeedback.Status = item.Status;
            existingFeedback.ApplicationId = item.ApplicationId;
            existingFeedback.CreatedAt = DateTime.UtcNow;

            if (item.Members != null)
            {
                var memberIds = item.Members.Select(m => m.Id).ToList();
                var pagedMembers = await UnitOfWork.MemberRepository
                    .GetListAsync(m => memberIds.Contains(m.Id)).ConfigureAwait(false);
                var newMembers = pagedMembers.Result.ToList();

                existingFeedback.Members.Clear();
                foreach (var member in newMembers)
                {
                    existingFeedback.Members.Add(member);
                }
            }

            var validationResult = await Validator.ValidateAsync(existingFeedback).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return OperationResult.InvalidData(validationResult);
            }

            var application = await UnitOfWork.ApplicationDataRepository.GetByIdAsync(item.ApplicationId).ConfigureAwait(false);
            if (application == null)
            {
                return OperationResult.NotFound(_localizer[nameof(ServiceResources.NotFound)]);
            }

            if (existingFeedback.Members != null && existingFeedback.Members.Count > 0)
            {
                var squadId = application.SquadId;
                var invalidMemberIds = existingFeedback.Members
                    .Where(m => m.SquadId != squadId)
                    .Select(m => m.Id)
                    .ToList();

                if (invalidMemberIds.Count > 0)
                {
                    return OperationResult.Conflict(_localizer[nameof(FeedbackResources.InvalidMembers)]);
                }
            }

            await Repository.UpdateAsync(existingFeedback, saveChanges: true).ConfigureAwait(false);

            return OperationResult.Complete();
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

        public async Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId)
        {
            return await Repository.GetMembersByApplicationIdAsync(applicationId).ConfigureAwait(false);
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
