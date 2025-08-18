using System.Globalization;
using Application.Tests.Helpers;
using AutoFixture;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.Validators;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;

namespace Application.Tests.Services
{
    public class FeedbackServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IFeedbackRepository> _feedbackRepositoryMock;
        private readonly Mock<IApplicationDataRepository> _applicationDataRepositoryMock;
        private readonly Mock<IMemberRepository> _memberRepositoryMock;
        private readonly Mock<IStringLocalizer<NotificationResources>> _notificationLocalizerMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly IStringLocalizerFactory _localizerFactory;
        private readonly FeedbackValidator _feedbackValidator;
        private readonly FeedbackService _feedbackService;
        private readonly Fixture _fixture;

        public FeedbackServiceTests()
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _feedbackRepositoryMock = new Mock<IFeedbackRepository>();
            _applicationDataRepositoryMock = new Mock<IApplicationDataRepository>();
            _memberRepositoryMock = new Mock<IMemberRepository>();
            _notificationLocalizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            _notificationServiceMock = new Mock<INotificationService>();
            _localizerFactory = LocalizerFactorHelper.Create();
            _feedbackValidator = new FeedbackValidator(_localizerFactory);

            _unitOfWorkMock.Setup(u => u.FeedbackRepository).Returns(_feedbackRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(_applicationDataRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(_memberRepositoryMock.Object);

            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _feedbackService = new FeedbackService(
                _unitOfWorkMock.Object,
                _localizerFactory,
                _notificationLocalizerMock.Object,
                _feedbackValidator,
                _notificationServiceMock.Object
            );
        }

        [Fact]
        public void CulturePropertyShouldGetAndSetCorrectValue()
        {
            CultureInfo expectedCulture = new("pt-BR");
            FeedbackResources.Culture = expectedCulture;
            CultureInfo result = FeedbackResources.Culture;
            Assert.Equal(expectedCulture, result);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            var feedback = new Feedback
            {
                Title = "",
                Description = "",
                ApplicationId = 0,
                Members = []
            };

            var result = await _feedbackService.CreateAsync(feedback);

            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(FeedbackResources.TitleRequired, result.Errors);
            Assert.Contains(FeedbackResources.ApplicationRequired, result.Errors);
            Assert.Contains(FeedbackResources.DescriptionRequired, result.Errors);

        }

        [Fact]
        public async Task CreateAsyncShouldReturnNotFoundWhenApplicationDoesNotExist()
        {
            var feedback = new Feedback
            {
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Members = []
            };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(feedback.ApplicationId)).ReturnsAsync((ApplicationData?)null);

            var result = await _feedbackService.CreateAsync(feedback);

            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Contains(FeedbackResources.NotFound, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenMemberNotInSquad()
        {
            var member = new Member { Id = 1, Name = "M", Role = "Dev", Cost = 1, Email = "m@x.com", SquadId = 99 };
            var feedback = new Feedback
            {
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Members = [member]
            };
            var app = new ApplicationData("App") { Id = 1, SquadId = 1, ProductOwner = "PO" };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(feedback.ApplicationId)).ReturnsAsync(app);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = [member] });

            var result = await _feedbackService.CreateAsync(feedback);

            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenValid()
        {
            var member = new Member { Id = 1, Name = "M", Role = "Dev", Cost = 1, Email = "m@x.com", SquadId = 1 };
            var feedback = new Feedback
            {
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Members = [member]
            };
            var app = new ApplicationData("App") { Id = 1, SquadId = 1, ProductOwner = "PO" };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(feedback.ApplicationId)).ReturnsAsync(app);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = [member] });
            _feedbackRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Feedback>(), true)).Returns(Task.CompletedTask);

            var result = await _feedbackService.CreateAsync(feedback);

            Assert.Equal(OperationStatus.Success, result.Status);
            _notificationServiceMock.Verify(n => n.NotifyFeedbackCreatedAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            var filter = _fixture.Create<FeedbackFilter>();
            var pagedResult = _fixture.Create<PagedResult<Feedback>>();
            _feedbackRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            var result = await _feedbackService.GetListAsync(filter);

            Assert.Equal(pagedResult, result);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenItemDoesNotExist()
        {
            int id = _fixture.Create<int>();
            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Feedback?)null);

            var result = await _feedbackService.GetItemAsync(id);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnSuccessWhenItemExists()
        {
            var feedback = new Feedback
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Members = []
            };
            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(feedback.Id)).ReturnsAsync(feedback);

            var result = await _feedbackService.GetItemAsync(feedback.Id);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenFeedbackDoesNotExist()
        {
            var feedback = new Feedback { Id = 1, Title = "Teste", Description = "Desc", ApplicationId = 1 };

            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(feedback.Id)).ReturnsAsync((Feedback?)null);

            var result = await _feedbackService.UpdateAsync(feedback);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            var app = new ApplicationData("App")
            {
                Id = 1,
                Name = "App",
                ProductOwner = "PO",
                SquadId = 1
            };
            var existing = new Feedback
            {
                Id = 1,
                Title = "Old",
                Description = "Old",
                ApplicationId = 1,
                Application = app,
                Members = []
            };
            var feedback = new Feedback
            {
                Id = 1,
                Title = "",
                Description = "",
                ApplicationId = 1,
                Application = app,
                Members = []
            };

            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(feedback.Id)).ReturnsAsync(existing);
            // Mock para evitar NullReference ao buscar membros
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = new List<Member>() });

            var result = await _feedbackService.UpdateAsync(feedback);

            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenApplicationDoesNotExist()
        {
            var app = new ApplicationData("App") { Id = 1, ProductOwner = "PO", SquadId = 1 };
            var existing = new Feedback
            {
                Id = 1,
                Title = "Old",
                Description = "Old",
                ApplicationId = 1,
                Application = app,
                Members = []
            };
            var feedback = new Feedback
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Application = app,
                Members = []
            };

            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(feedback.Id)).ReturnsAsync(existing);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(feedback.ApplicationId)).ReturnsAsync((ApplicationData?)null);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = new List<Member>() });

            var result = await _feedbackService.UpdateAsync(feedback);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenMemberNotInSquad()
        {
            var member = new Member { Id = 1, Name = "M", Role = "Dev", Cost = 1, Email = "m@x.com", SquadId = 99 };
            var existing = new Feedback { Id = 1, Title = "Old", Description = "Old", ApplicationId = 1, Members = new List<Member>() };
            var feedback = new Feedback { Id = 1, Title = "Teste", Description = "Desc", ApplicationId = 1, Members = [member] };
            var app = new ApplicationData("App") { Id = 1, SquadId = 1, ProductOwner = "PO" };

            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(feedback.Id)).ReturnsAsync(existing);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(feedback.ApplicationId)).ReturnsAsync(app);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = [member] });

            var result = await _feedbackService.UpdateAsync(feedback);

            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenValid()
        {
            var member = new Member { Id = 1, Name = "M", Role = "Dev", Cost = 1, Email = "m@x.com", SquadId = 1 };
            var existing = new Feedback { Id = 1, Title = "Old", Description = "Old", ApplicationId = 1, Members = new List<Member>() };
            var feedback = new Feedback { Id = 1, Title = "Teste", Description = "Desc", ApplicationId = 1, Members = [member] };
            var app = new ApplicationData("App") { Id = 1, SquadId = 1, ProductOwner = "PO" };

            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(feedback.Id)).ReturnsAsync(existing);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(feedback.ApplicationId)).ReturnsAsync(app);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = [member] });
            _feedbackRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Feedback>(), true)).Returns(Task.CompletedTask);

            var result = await _feedbackService.UpdateAsync(feedback);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenItemDoesNotExist()
        {
            int id = _fixture.Create<int>();
            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Feedback?)null);

            var result = await _feedbackService.DeleteAsync(id);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenSuccessful()
        {
            var feedback = new Feedback
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Members = []
            };
            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(feedback.Id)).ReturnsAsync(feedback);
            _feedbackRepositoryMock.Setup(r => r.DeleteAsync(feedback, true)).Returns(Task.CompletedTask);

            var result = await _feedbackService.DeleteAsync(feedback.Id);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public void FeedbackServiceInvalidMembersResourceReturnsExpectedString()
        {
            // Arrange
            var expected = FeedbackResources.InvalidMembers;

            // Act
            var actual = FeedbackResources.InvalidMembers;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FeedbackResourceStatusRequiredPropertyReturnsExpectedString()
        {
            // Arrange
            var expected = FeedbackResources.StatusRequired;

            // Act
            var actual = FeedbackResources.StatusRequired;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FeedbackResourceConstructorCanBeInvoked()
        {
            // Act
            var resource = Activator.CreateInstance(typeof(FeedbackResources), true);

            // Assert
            Assert.NotNull(resource);
        }

        public async Task<IEnumerable<Member>> BuscarMembrosPorAplicacaoAsync(int applicationId)
        {
            return await _feedbackService.GetMembersByApplicationIdAsync(applicationId).ConfigureAwait(false);
        }

        public async Task NotificateChangesStatusFeedbackAsync(Feedback existingFeedback)
        {
            ArgumentNullException.ThrowIfNull(existingFeedback);

            await _notificationServiceMock.Object.NotifyFeedbackStatusChangeAsync(existingFeedback.Id).ConfigureAwait(false);
        }

        [Fact]
        public async Task UpdateAsyncShouldNotifyStatusChangeWhenStatusIsChanged()
        {
            // Arrange
            var member = new Member
            {
                Id = 1,
                Name = "M",
                Role = "Developer", 
                Cost = 1000,     
                Email = "m@example.com", 
                SquadId = 1
            };
            var app = new ApplicationData("App") { Id = 1, SquadId = 1, ProductOwner = "PO" };
            var existing = new Feedback { Id = 1, Title = "Old", Description = "Old", ApplicationId = 1, Status = FeedbackStatus.Open, Members = [member], Application = app };
            var feedback = new Feedback { Id = 1, Title = "Teste", Description = "Desc", ApplicationId = 1, Status = FeedbackStatus.Closed, Members = [member], Application = app };

            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(feedback.Id)).ReturnsAsync(existing);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(feedback.ApplicationId)).ReturnsAsync(app);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = [member] });
            _feedbackRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Feedback>(), true)).Returns(Task.CompletedTask);

            // Act
            var result = await _feedbackService.UpdateAsync(feedback);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            _notificationServiceMock.Verify(n => n.NotifyFeedbackStatusChangeAsync(existing.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateAsyncShouldNotifyRemovedMembersWhenMembersAreRemoved()
        {
            // Arrange
            var member1 = new Member
            {
                Id = 1,
                Name = "M1",
                SquadId = 1,
                Role = "Developer", 
                Cost = 1000,        
                Email = "m1@example.com" 
            };
            var member2 = new Member
            {
                Id = 2,
                Name = "M2",
                SquadId = 1,
                Role = "Developer", 
                Cost = 1000,       
                Email = "m2@example.com" 
            };
            var app = new ApplicationData("App") { Id = 1, SquadId = 1, ProductOwner = "PO" };
            var existing = new Feedback { Id = 1, Title = "Old", Description = "Old", ApplicationId = 1, Status = FeedbackStatus.Open, Members = new List<Member> { member1, member2 }, Application = app };
            var feedback = new Feedback { Id = 1, Title = "Teste", Description = "Desc", ApplicationId = 1, Status = FeedbackStatus.Open, Members = new List<Member> { member1 }, Application = app };

            _feedbackRepositoryMock.Setup(r => r.GetByIdAsync(feedback.Id)).ReturnsAsync(existing);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(feedback.ApplicationId)).ReturnsAsync(app);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = new List<Member> { member1 } });
            _feedbackRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Feedback>(), true)).Returns(Task.CompletedTask);

            _notificationLocalizerMock.Setup(l => l["FeedbackRemoveMember", member2.Name, existing.Title])
                .Returns(new LocalizedString("FeedbackRemoveMember", $"Removido: {member2.Name} do {existing.Title}"));

            // Act
            var result = await _feedbackService.UpdateAsync(feedback);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            _notificationServiceMock.Verify(n => n.NotifyMembersAsync(It.Is<IEnumerable<Member>>(m => m.First().Id == member2.Id), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetMembersByApplicationIdAsyncShouldCallRepository()
        {
            // Arrange
            var applicationId = 1;
            var members = new List<Member>
            {
                new Member
                {
                    Id = 1,
                    Name = "M",
                    SquadId = 1,
                    Role = "Developer",
                    Cost = 1000,   
                    Email = "m@example.com" 
                }
            };
            _feedbackRepositoryMock.Setup(r => r.GetMembersByApplicationIdAsync(applicationId)).ReturnsAsync(members);

            // Act
            var result = await _feedbackService.GetMembersByApplicationIdAsync(applicationId);

            // Assert
            Assert.Equal(members, result);
            _feedbackRepositoryMock.Verify(r => r.GetMembersByApplicationIdAsync(applicationId), Times.Once);
        }
    }
}