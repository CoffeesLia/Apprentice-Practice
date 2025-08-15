//using Xunit;
//using Moq;
//using FluentValidation;
//using FluentValidation.Results;
//using Microsoft.Extensions.Localization;
//using Stellantis.ProjectName.Application.Interfaces;
//using Stellantis.ProjectName.Application.Interfaces.Repositories;
//using Stellantis.ProjectName.Application.Interfaces.Services;
//using Stellantis.ProjectName.Application.Models;
//using Stellantis.ProjectName.Application.Models.Filters;
//using Stellantis.ProjectName.Application.Resources;
//using Stellantis.ProjectName.Domain.Entities;
//using Stellantis.ProjectName.Application.Services;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace Application.Tests.Services
//{
//    public class FeedbackServiceTests
//    {
//        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
//        private readonly Mock<IFeedbackRepository> _feedbackRepositoryMock = new();
//        private readonly Mock<IMemberRepository> _memberRepositoryMock = new();
//        private readonly Mock<IApplicationDataRepository> _applicationDataRepositoryMock = new();
//        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock = new();
//        private readonly Mock<IStringLocalizer<NotificationResources>> _notificationLocalizerMock = new();
//        private readonly Mock<IValidator<Feedback>> _validatorMock = new();
//        private readonly Mock<INotificationService> _notificationServiceMock = new();
//        private readonly Mock<IStringLocalizer> _localizerMock = new();

//        private FeedbackService CreateService()
//        {
//            _unitOfWorkMock.SetupGet(u => u.FeedbackRepository).Returns(_feedbackRepositoryMock.Object);
//            _unitOfWorkMock.SetupGet(u => u.MemberRepository).Returns(_memberRepositoryMock.Object);
//            _unitOfWorkMock.SetupGet(u => u.ApplicationDataRepository).Returns(_applicationDataRepositoryMock.Object);
//            _localizerFactoryMock.Setup(f => f.Create(typeof(FeedbackResources))).Returns(_localizerMock.Object);
//            return new FeedbackService(
//                _unitOfWorkMock.Object,
//                _localizerFactoryMock.Object,
//                _notificationLocalizerMock.Object,
//                _validatorMock.Object,
//                _notificationServiceMock.Object
//            );
//        }

//        [Fact]
//        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
//        {
//            var feedback = new Feedback { Title = "t", Description = "d", ApplicationId = 1 };
//            var validationResult = new ValidationResult(new[] { new ValidationFailure("Title", "Required") });
//            _validatorMock.Setup(v => v.ValidateAsync(feedback, default)).ReturnsAsync(validationResult);

//            var service = CreateService();
//            var result = await service.CreateAsync(feedback);

//            Assert.Equal(OperationResult.InvalidData(validationResult).Status, result.Status);
//        }

//        [Fact]
//        public async Task CreateAsyncShouldReturnNotFoundWhenApplicationDoesNotExist()
//        {
//            var feedback = new Feedback { Title = "t", Description = "d", ApplicationId = 1 };
//            _validatorMock.Setup(v => v.ValidateAsync(feedback, default)).ReturnsAsync(new ValidationResult());
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ApplicationData)null);

//            var service = CreateService();
//            var result = await service.CreateAsync(feedback);

//            Assert.Equal(OperationResult.NotFound(string.Empty).Status, result.Status);
//        }

//        [Fact]
//        public async Task CreateAsyncShouldReturnConflictWhenMembersNotInSquad()
//        {
//            var feedback = new Feedback
//            {
//                Members = [new() { Id = 1, SquadId = 2, Name = "A", Role = "Dev", Cost = 1, Email = "a@a.com" }],
//                ApplicationId = 1
//            };
//            _validatorMock.Setup(v => v.ValidateAsync(feedback, default)).ReturnsAsync(new ValidationResult());
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new ApplicationData("App") { SquadId = 1 });
//            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<MemberFilter>())).ReturnsAsync(new PagedResult<Member> { Result = new List<Member> { new Member { Id = 1, SquadId = 2, Name = "A", Role = "Dev", Cost = 1, Email = "a@a.com" } }, Page = 1, PageSize = 10, Total = 1 });

//            var service = CreateService();
//            var result = await service.CreateAsync(feedback);

//            Assert.Equal(OperationResult.Conflict(string.Empty).Status, result.Status);
//        }

//        [Fact]
//        public async Task CreateAsyncShouldNotifyWhenSuccess()
//        {
//            var feedback = new Feedback { Title = "t", Description = "d", ApplicationId = 1 };
//            _validatorMock.Setup(v => v.ValidateAsync(feedback, default)).ReturnsAsync(new ValidationResult());
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new ApplicationData("App") { SquadId = 1 });
//            OperationResult.Setup(string.Empty)(r => r.CreateAsync(feedback)).ReturnsAsync(OperationResult.Complete());
//            _notificationServiceMock.Setup(n => n.NotifyFeedbackCreatedAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

//            var service = CreateService();
//            var result = await service.CreateAsync(feedback);

//            Assert.Equal(OperationResult.Complete().Status, result.Status);
//            _notificationServiceMock.Verify(n => n.NotifyFeedbackCreatedAsync(It.IsAny<int>()), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateAsyncShouldReturnNotFoundWhenFeedbackDoesNotExist()
//        {
//            var feedback = new Feedback { Id = 1 };
//            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Feedback)null);

//            var service = CreateService();
//            var result = await service.UpdateAsync(feedback);

//            Assert.Equal(OperationResult.NotFound(string.Empty).Status, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
//        {
//            var feedback = new Feedback { Id = 1 };
//            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Feedback { Id = 1 });
//            var validationResult = new ValidationResult(new[] { new ValidationFailure("Title", "Required") });
//            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Feedback>(), default)).ReturnsAsync(validationResult);

//            var service = CreateService();
//            var result = await service.UpdateAsync(feedback);

//            Assert.Equal(OperationResult.InvalidData(validationResult).Status, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncShouldReturnNotFoundWhenApplicationDoesNotExist()
//        {
//            var feedback = new Feedback { Id = 1, ApplicationId = 1 };
//            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Feedback { Id = 1 });
//            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Feedback>(), default)).ReturnsAsync(new ValidationResult());
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ApplicationData)null);

//            var service = CreateService();
//            var result = await service.UpdateAsync(feedback);

//            Assert.Equal(OperationResult.NotFound(string.Empty).Status, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncShouldReturnConflictWhenMembersNotInSquad()
//        {
//            var feedback = new Feedback
//            {
//                Id = 1,
//                ApplicationId = 1,
//                Members = new List<Member> { new Member { Id = 1, SquadId = 2, Name = "A", Role = "Dev", Cost = 1, Email = "a@a.com" } }
//            };
//            var existingFeedback = new Feedback
//            {
//                Id = 1,
//                Members = new List<Member> { new Member { Id = 1, SquadId = 2, Name = "A", Role = "Dev", Cost = 1, Email = "a@a.com" } }
//            };
//            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingFeedback);
//            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Feedback>(), default)).ReturnsAsync(new ValidationResult());
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new ApplicationData("App") { SquadId = 1 });
//            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<MemberFilter>())).ReturnsAsync(new PagedResult<Member> { Result = new List<Member> { new Member { Id = 1, SquadId = 2, Name = "A", Role = "Dev", Cost = 1, Email = "a@a.com" } }, Page = 1, PageSize = 10, Total = 1 });

//            var service = CreateService();
//            var result = await service.UpdateAsync(feedback);

//            Assert.Equal(OperationResult.Conflict(string.Empty).Status, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncShouldNotifyWhenStatusChanged()
//        {
//            var feedback = new Feedback { Id = 1, Status = FeedbackStatus.Closed, ApplicationId = 1 };
//            var existingFeedback = new Feedback { Id = 1, Status = FeedbackStatus.Open };
//            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingFeedback);
//            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Feedback>(), default)).ReturnsAsync(new ValidationResult());
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new ApplicationData("App") { SquadId = 1 });
//            _feedbackRepositoryMock.Setup(r => r.UpdateAsync(existingFeedback, true)).Returns(Task.CompletedTask);
//            _notificationServiceMock.Setup(n => n.NotifyFeedbackStatusChangeAsync(1)).Returns(Task.CompletedTask);

//            var service = CreateService();
//            var result = await service.UpdateAsync(feedback);

//            Assert.Equal(OperationResult.Complete().Status, result.Status);
//            _notificationServiceMock.Verify(n => n.NotifyFeedbackStatusChangeAsync(1), Times.Once);
//        }

//        [Fact]
//        public async Task GetItemAsyncShouldReturnNotFoundWhenFeedbackDoesNotExist()
//        {
//            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Feedback)null);

//            var service = CreateService();
//            var result = await service.GetItemAsync(1);

//            Assert.Equal(OperationResult.NotFound(string.Empty).Status, result.Status);
//        }

//        [Fact]
//        public async Task GetItemAsyncShouldReturnCompleteWhenFeedbackExists()
//        {
//            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Feedback { Id = 1 });

//            var service = CreateService();
//            var result = await service.GetItemAsync(1);

//            Assert.Equal(OperationResult.Complete().Status, result.Status);
//        }

//        [Fact]
//        public async Task GetListAsyncShouldReturnPagedResult()
//        {
//            var pagedResult = new PagedResult<Feedback> { Result = new List<Feedback>(), Page = 1, PageSize = 10, Total = 0 };
//            _feedbackRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<FeedbackFilter>())).ReturnsAsync(pagedResult);

//            var service = CreateService();
//            var result = await service.GetListAsync(new FeedbackFilter());

//            Assert.Equal(pagedResult, result);
//        }

//        [Fact]
//        public async Task GetMembersByApplicationIdAsyncShouldReturnMembers()
//        {
//            var members = new List<Member> { new Member { Id = 1, Name = "A", SquadId = 1, Role = "Dev", Cost = 1, Email = "a@a.com" } };
//            _feedbackRepositoryMock.Setup(r => r.GetMembersByApplicationIdAsync(1)).ReturnsAsync(members);

//            var service = CreateService();
//            var result = await service.GetMembersByApplicationIdAsync(1);

//            Assert.Equal(members, result);
//        }

//        [Fact]
//        public async Task DeleteAsyncShouldReturnNotFoundWhenFeedbackDoesNotExist()
//        {
//            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Feedback)null);

//            var service = CreateService();
//            var result = await service.DeleteAsync(1);

//            Assert.Equal(OperationResult.NotFound(string.Empty).Status, result.Status);
//        }

//        [Fact]
//        public async Task DeleteAsyncShouldCallBaseDeleteWhenFeedbackExists()
//        {
//            var feedback = new Feedback { Id = 1 };
//            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(feedback);
//            _feedbackRepositoryMock.Setup(r => r.DeleteAsync(feedback)).ReturnsAsync(OperationResult.Complete());

//            var service = CreateService();
//            var result = await service.DeleteAsync(1);

//            Assert.Equal(OperationResult.Complete().Status, result.Status);
//        }
//    }
//}