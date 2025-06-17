//using Application.Tests.Helpers;
//using AutoFixture;
//using Microsoft.Extensions.Localization;
//using Moq;
//using Stellantis.ProjectName.Application.Interfaces;
//using Stellantis.ProjectName.Application.Interfaces.Repositories;
//using Stellantis.ProjectName.Application.Models;
//using Stellantis.ProjectName.Application.Models.Filters;
//using Stellantis.ProjectName.Application.Resources;
//using Stellantis.ProjectName.Application.Services;
//using Stellantis.ProjectName.Application.Validators;
//using Stellantis.ProjectName.Domain.Entities;
//using System.Globalization;
//using Xunit;

//namespace Application.Tests.Services
//{
//    public class FeedbacksServiceTests
//    {
//        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
//        private readonly Mock<IFeedbacksRepository> _feedbacksRepositoryMock;
//        private readonly Mock<IApplicationDataRepository> _applicationDataRepositoryMock;
//        private readonly FeedbacksService _feedbacksService;
//        private readonly Fixture _fixture;
//        private readonly IStringLocalizerFactory _localizerFactory;
//        private readonly FeedbacksValidator _feedbacksValidator;

//        public FeedbacksServiceTests()
//        {
//            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
//            _unitOfWorkMock = new Mock<IUnitOfWork>();
//            _feedbacksRepositoryMock = new Mock<IFeedbacksRepository>();
//            _applicationDataRepositoryMock = new Mock<IApplicationDataRepository>();
//            _localizerFactory = LocalizerFactorHelper.Create();
//            _feedbacksValidator = new FeedbacksValidator(_localizerFactory);

//            _unitOfWorkMock.Setup(u => u.FeedbacksRepository).Returns(_feedbacksRepositoryMock.Object);
//            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(_applicationDataRepositoryMock.Object);

//            _fixture = new Fixture();
//            _fixture.Behaviors
//                .OfType<ThrowingRecursionBehavior>()
//                .ToList()
//                .ForEach(b => _fixture.Behaviors.Remove(b));
//            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

//            _feedbacksService = new FeedbacksService(_unitOfWorkMock.Object, _localizerFactory, _feedbacksValidator);
//        }

//        [Fact]
//        public void CulturePropertyShouldGetAndSetCorrectValue()
//        {
//            // Arrange
//            CultureInfo expectedCulture = new("pt-BR");

//            // Act
//            FeedbacksResources.Culture = expectedCulture;
//            CultureInfo result = FeedbacksResources.Culture;

//            // Assert
//            Assert.Equal(expectedCulture, result);
//        }

//        [Fact]
//        public async Task CreateAsyncWhenValidationFails()
//        {
//            // Arrange
//            Feedback feedbacks = _fixture.Build<Feedback>()
//                .With(i => i.Title, string.Empty)
//                .With(i => i.Description, string.Empty)
//                .With(i => i.ApplicationId, 0) 

//                .Create();

//            // Act
//            OperationResult result = await _feedbacksService.CreateAsync(feedbacks);

//            // Assert
//            Assert.Equal(OperationStatus.InvalidData, result.Status);
//            Assert.Contains(FeedbacksResources.TitleRequired, result.Errors);
//            Assert.Contains(FeedbacksResources.ApplicationRequired, result.Errors);
//            Assert.Contains(FeedbacksResources.DescriptionRequired, result.Errors);
//        }

//        [Fact]
//        public async Task CreateAsyncWhenApplicationNotFound()
//        {
//            // Arrange
//            Feedback feedbacks = _fixture.Create<Feedback>();
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(feedbacks.ApplicationId)).ReturnsAsync((ApplicationData?)null);

//            // Act
//            OperationResult result = await _feedbacksService.CreateAsync(feedbacks);

//            // Assert
//            Assert.Equal(OperationStatus.NotFound, result.Status);
//        }

//        [Fact]
//        public async Task CreateAsyncWhenInvalidMembers()
//        {
//            // Arrange
//            var feedbacks = _fixture.Create<Feedback>();
//            var app = _fixture.Build<ApplicationData>()
//                .With(a => a.Id, feedbacks.ApplicationId)
//                .With(a => a.Squads, [])
//                .Create();

//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(feedbacks.ApplicationId)).ReturnsAsync(app);

//            // Act
//            OperationResult result = await _feedbacksService.CreateAsync(feedbacks);

//            // Assert
//            Assert.Equal(OperationStatus.Conflict, result.Status);
//            Assert.Contains(FeedbacksResources.InvalidMembers, result.Errors);
//        }

//        [Fact]
//        public async Task CreateAsyncWhenSuccessful()
//        {
//            // Arrange
//            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
//            var squad = _fixture.Build<Squad>().With(s => s.Members, [member]).Create();
//            var app = _fixture.Build<ApplicationData>().With(a => a.Squads, [squad]).Create();
//            var feedbacks = _fixture.Build<Feedback>()
//                .With(i => i.ApplicationId, app.Id)
//                .With(i => i.Members, [member])
//                .Create();

//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

//            // Act
//            OperationResult result = await _feedbacksService.CreateAsync(feedbacks);

//            // Assert
//            Assert.Equal(OperationStatus.Success, result.Status);
//        }

//        [Fact]
//        public async Task GetListAsyncShouldReturnPagedResult()
//        {
//            // Arrange
//            filter filter = _fixture.Create<filter>();
//            PagedResult<Feedback> pagedResult = _fixture.Create<PagedResult<Feedback>>();
//            _feedbacksRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

//            // Act
//            PagedResult<Feedback> result = await _feedbacksService.GetListAsync(filter);

//            // Assert
//            Assert.Equal(pagedResult, result);
//        }

//        [Fact]
//        public async Task GetItemAsyncWhenItemDoesNotExist()
//        {
//            // Arrange
//            int id = _fixture.Create<int>();
//            _feedbacksRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Feedback?)null);

//            // Act
//            OperationResult result = await _feedbacksService.GetItemAsync(id);

//            // Assert
//            Assert.Equal(OperationStatus.NotFound, result.Status);
//        }

//        [Fact]
//        public async Task GetItemAsyncWhenItemExists()
//        {
//            // Arrange
//            Feedback feedbacks = _fixture.Create<Feedback>();
//            _feedbacksRepositoryMock.Setup(r => r.GetByIdAsync(feedbacks.Id)).ReturnsAsync(feedbacks);

//            // Act
//            OperationResult result = await _feedbacksService.GetItemAsync(feedbacks.Id);

//            // Assert
//            Assert.Equal(OperationStatus.Success, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncWhenValidationFails()
//        {
//            // Arrange
//            Feedback feedbacks = _fixture.Build<Feedback>()
//                .With(i => i.Title, string.Empty)
//                .With(i => i.Description, string.Empty)
//                .Create();

//            // Act
//            OperationResult result = await _feedbacksService.UpdateAsync(feedbacks);

//            // Assert
//            Assert.Equal(OperationStatus.InvalidData, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncWhenFeedbacksNotFound()
//        {
//            // Arrange
//            Feedback feedbacks = _fixture.Create<Feedback>();
//            _feedbacksRepositoryMock.Setup(r => r.GetByIdAsync(feedbacks.Id)).ReturnsAsync((Feedback?)null);

//            // Act
//            OperationResult result = await _feedbacksService.UpdateAsync(feedbacks);

//            // Assert
//            Assert.Equal(OperationStatus.NotFound, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncWhenApplicationNotFound()
//        {
//            // Arrange
//            Feedback feedbacks = _fixture.Create<Feedback>();
//            _feedbacksRepositoryMock.Setup(r => r.GetByIdAsync(feedbacks.Id)).ReturnsAsync(feedbacks);
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(feedbacks.ApplicationId)).ReturnsAsync((ApplicationData?)null);

//            // Act
//            OperationResult result = await _feedbacksService.UpdateAsync(feedbacks);

//            // Assert
//            Assert.Equal(OperationStatus.NotFound, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncWhenInvalidMembers()
//        {
//            // Arrange
//            var feedbacks = _fixture.Create<Feedback>();
//            _feedbacksRepositoryMock.Setup(r => r.GetByIdAsync(feedbacks.Id)).ReturnsAsync(feedbacks);

//            var app = _fixture.Build<ApplicationData>()
//                .With(a => a.Id, feedbacks.ApplicationId)
//                .With(a => a.Squads, []) 
//                .Create();

//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(feedbacks.ApplicationId)).ReturnsAsync(app);

//            // Act
//            OperationResult result = await _feedbacksService.UpdateAsync(feedbacks);

//            // Assert
//            Assert.Equal(OperationStatus.Conflict, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncWhenSuccessful()
//        {
//            // Arrange
//            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
//            var squad = _fixture.Build<Squad>().With(s => s.Members, [member]).Create();
//            var app = _fixture.Build<ApplicationData>().With(a => a.Squads, [squad]).Create();
//            var feedbacks = _fixture.Build<Feedback>()
//                .With(i => i.ApplicationId, app.Id)
//                .With(i => i.Members, [member])
//                .Create();

//            _feedbacksRepositoryMock.Setup(r => r.GetByIdAsync(feedbacks.Id)).ReturnsAsync(feedbacks);
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

//            // Act
//            OperationResult result = await _feedbacksService.UpdateAsync(feedbacks);

//            // Assert
//            Assert.Equal(OperationStatus.Success, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncWhenStatusSetToFechadoShouldSetClosedAt()
//        {
//            // Arrange
//            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
//            var squad = _fixture.Build<Squad>().With(s => s.Members, [member]).Create();
//            var app = _fixture.Build<ApplicationData>().With(a => a.Squads, [squad]).Create();

//            var feedbacks = _fixture.Build<Feedback>()
//                .With(i => i.FeedbackStatus, Status.Open)
//                .With(i => i.ClosedAt, (DateTime?)null)
//                .With(i => i.ApplicationId, app.Id)
//                .With(i => i.Members, [member])
//                .Create();

//            var updatedFeedbacks = _fixture.Build<Feedback>()
//                .With(i => i.Id, feedbacks.Id)
//                .With(i => i.FeedbackStatus, Status.Closed)
//                .With(i => i.ClosedAt, (DateTime?)null)
//                .With(i => i.ApplicationId, app.Id)
//                .With(i => i.Members, [member])
//                .Create();

//            _feedbacksRepositoryMock.Setup(r => r.GetByIdAsync(feedbacks.Id)).ReturnsAsync(feedbacks);
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

//            // Act
//            OperationResult result = await _feedbacksService.UpdateAsync(updatedFeedbacks);

//            // Assert
//            Assert.Equal(OperationStatus.Success, result.Status);
//            Assert.NotNull(feedbacks.ClosedAt);
//        }

//        [Fact]
//        public async Task UpdateAsyncWhenStatusSetToReabertoShouldClearClosedAt()
//        {
//            // Arrange
//            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
//            var squad = _fixture.Build<Squad>().With(s => s.Members, [member]).Create();
//            var app = _fixture.Build<ApplicationData>().With(a => a.Squads, [squad]).Create();

//            var feedbacks = _fixture.Build<Feedback>()
//                .With(i => i.FeedbackStatus, Status.Closed)
//                .With(i => i.ClosedAt, DateTime.UtcNow)
//                .With(i => i.ApplicationId, app.Id)
//                .With(i => i.Members, [member])
//                .Create();

//            var updatedFeedbacks = _fixture.Build<Feedback>()
//                .With(i => i.Id, feedbacks.Id)
//                .With(i => i.FeedbackStatus, Status.Reopened)
//                .With(i => i.ClosedAt, feedbacks.ClosedAt)
//                .With(i => i.ApplicationId, app.Id)
//                .With(i => i.Members, [member])
//                .Create();

//            _feedbacksRepositoryMock.Setup(r => r.GetByIdAsync(feedbacks.Id)).ReturnsAsync(feedbacks);
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

//            // Act
//            OperationResult result = await _feedbacksService.UpdateAsync(updatedFeedbacks);

//            // Assert
//            Assert.Equal(OperationStatus.Success, result.Status);
//            Assert.Null(feedbacks.ClosedAt);
//        }

//        [Fact]
//        public async Task DeleteAsyncWhenItemDoesNotExist()
//        {
//            // Arrange
//            int id = _fixture.Create<int>();
//            _feedbacksRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Feedback?)null);

//            // Act
//            OperationResult result = await _feedbacksService.DeleteAsync(id);

//            // Assert
//            Assert.Equal(OperationStatus.NotFound, result.Status);
//        }

//        [Fact]
//        public async Task DeleteAsyncWhenSuccessful()
//        {
//            // Arrange
//            Feedback feedbacks = _fixture.Create<Feedback>();
//            _feedbacksRepositoryMock.Setup(r => r.GetByIdAsync(feedbacks.Id)).ReturnsAsync(feedbacks);

//            // Act
//            OperationResult result = await _feedbacksService.DeleteAsync(feedbacks.Id);

//            // Assert
//            Assert.Equal(OperationStatus.Success, result.Status);
//        }
//    }
//}