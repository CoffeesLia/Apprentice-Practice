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
//    public class ImprovementServiceTests
//    {
//        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
//        private readonly Mock<IImprovementRepository> _improvementRepositoryMock;
//        private readonly Mock<IApplicationDataRepository> _applicationDataRepositoryMock;
//        private readonly ImprovementService _improvementService;
//        private readonly Fixture _fixture;
//        private readonly IStringLocalizerFactory _localizerFactory;
//        private readonly ImprovementValidator _improvementValidator;

//        public ImprovementServiceTests()
//        {
//            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
//            _unitOfWorkMock = new Mock<IUnitOfWork>();
//            _improvementRepositoryMock = new Mock<IImprovementRepository>();
//            _applicationDataRepositoryMock = new Mock<IApplicationDataRepository>();
//            _localizerFactory = LocalizerFactorHelper.Create();
//            _improvementValidator = new ImprovementValidator(_localizerFactory);

//            _unitOfWorkMock.Setup(u => u.ImprovementRepository).Returns(_improvementRepositoryMock.Object);
//            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(_applicationDataRepositoryMock.Object);

//            _fixture = new Fixture();
//            _fixture.Behaviors
//                .OfType<ThrowingRecursionBehavior>()
//                .ToList()
//                .ForEach(b => _fixture.Behaviors.Remove(b));
//            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

//            _improvementService = new ImprovementService(_unitOfWorkMock.Object, _localizerFactory, _improvementValidator);
//        }

//        [Fact]
//        public void CulturePropertyShouldGetAndSetCorrectValue()
//        {
//            // Arrange
//            CultureInfo expectedCulture = new("pt-BR");

//            // Act
//            ImprovementResources.Culture = expectedCulture;
//            CultureInfo result = ImprovementResources.Culture;

//            // Assert
//            Assert.Equal(expectedCulture, result);
//        }

//        [Fact]
//        public async Task CreateAsyncWhenValidationFails()
//        {
//            // Arrange
//            Improvement improvement = _fixture.Build<Improvement>()
//                .With(i => i.Title, string.Empty)
//                .With(i => i.Description, string.Empty)
//                .With(i => i.ApplicationId, 0) 

//                .Create();

//            // Act
//            OperationResult result = await _improvementService.CreateAsync(improvement);

//            // Assert
//            Assert.Equal(OperationStatus.InvalidData, result.Status);
//            Assert.Contains(ImprovementResources.TitleRequired, result.Errors);
//            Assert.Contains(ImprovementResources.ApplicationRequired, result.Errors);
//            Assert.Contains(ImprovementResources.DescriptionRequired, result.Errors);
//        }

//        [Fact]
//        public async Task CreateAsyncWhenApplicationNotFound()
//        {
//            // Arrange
//            Improvement improvement = _fixture.Create<Improvement>();
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(improvement.ApplicationId)).ReturnsAsync((ApplicationData?)null);

//            // Act
//            OperationResult result = await _improvementService.CreateAsync(improvement);

//            // Assert
//            Assert.Equal(OperationStatus.NotFound, result.Status);
//        }

//        [Fact]
//        public async Task CreateAsyncWhenInvalidMembers()
//        {
//            // Arrange
//            var improvement = _fixture.Create<Improvement>();
//            var app = _fixture.Build<ApplicationData>()
//                .With(a => a.Id, improvement.ApplicationId)
//                .With(a => a.Squads, [])
//                .Create();

//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(improvement.ApplicationId)).ReturnsAsync(app);

//            // Act
//            OperationResult result = await _improvementService.CreateAsync(improvement);

//            // Assert
//            Assert.Equal(OperationStatus.Conflict, result.Status);
//            Assert.Contains(ImprovementResources.InvalidMembers, result.Errors);
//        }

//        [Fact]
//        public async Task CreateAsyncWhenSuccessful()
//        {
//            // Arrange
//            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
//            var squad = _fixture.Build<Squad>().With(s => s.Members, [member]).Create();
//            var app = _fixture.Build<ApplicationData>().With(a => a.Squads, [squad]).Create();
//            var improvement = _fixture.Build<Improvement>()
//                .With(i => i.ApplicationId, app.Id)
//                .With(i => i.Members, [member])
//                .Create();

//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

//            // Act
//            OperationResult result = await _improvementService.CreateAsync(improvement);

//            // Assert
//            Assert.Equal(OperationStatus.Success, result.Status);
//        }

//        [Fact]
//        public async Task GetListAsyncShouldReturnPagedResult()
//        {
//            // Arrange
//            ImprovementFilter filter = _fixture.Create<ImprovementFilter>();
//            PagedResult<Improvement> pagedResult = _fixture.Create<PagedResult<Improvement>>();
//            _improvementRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

//            // Act
//            PagedResult<Improvement> result = await _improvementService.GetListAsync(filter);

//            // Assert
//            Assert.Equal(pagedResult, result);
//        }

//        [Fact]
//        public async Task GetItemAsyncWhenItemDoesNotExist()
//        {
//            // Arrange
//            int id = _fixture.Create<int>();
//            _improvementRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Improvement?)null);

//            // Act
//            OperationResult result = await _improvementService.GetItemAsync(id);

//            // Assert
//            Assert.Equal(OperationStatus.NotFound, result.Status);
//        }

//        [Fact]
//        public async Task GetItemAsyncWhenItemExists()
//        {
//            // Arrange
//            Improvement improvement = _fixture.Create<Improvement>();
//            _improvementRepositoryMock.Setup(r => r.GetByIdAsync(improvement.Id)).ReturnsAsync(improvement);

//            // Act
//            OperationResult result = await _improvementService.GetItemAsync(improvement.Id);

//            // Assert
//            Assert.Equal(OperationStatus.Success, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncWhenValidationFails()
//        {
//            // Arrange
//            Improvement improvement = _fixture.Build<Improvement>()
//                .With(i => i.Title, string.Empty)
//                .With(i => i.Description, string.Empty)
//                .Create();

//            // Act
//            OperationResult result = await _improvementService.UpdateAsync(improvement);

//            // Assert
//            Assert.Equal(OperationStatus.InvalidData, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncWhenImprovementNotFound()
//        {
//            // Arrange
//            Improvement improvement = _fixture.Create<Improvement>();
//            _improvementRepositoryMock.Setup(r => r.GetByIdAsync(improvement.Id)).ReturnsAsync((Improvement?)null);

//            // Act
//            OperationResult result = await _improvementService.UpdateAsync(improvement);

//            // Assert
//            Assert.Equal(OperationStatus.NotFound, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncWhenApplicationNotFound()
//        {
//            // Arrange
//            Improvement improvement = _fixture.Create<Improvement>();
//            _improvementRepositoryMock.Setup(r => r.GetByIdAsync(improvement.Id)).ReturnsAsync(improvement);
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(improvement.ApplicationId)).ReturnsAsync((ApplicationData?)null);

//            // Act
//            OperationResult result = await _improvementService.UpdateAsync(improvement);

//            // Assert
//            Assert.Equal(OperationStatus.NotFound, result.Status);
//        }

//        [Fact]
//        public async Task UpdateAsyncWhenInvalidMembers()
//        {
//            // Arrange
//            var improvement = _fixture.Create<Improvement>();
//            _improvementRepositoryMock.Setup(r => r.GetByIdAsync(improvement.Id)).ReturnsAsync(improvement);

//            var app = _fixture.Build<ApplicationData>()
//                .With(a => a.Id, improvement.ApplicationId)
//                .With(a => a.Squads, []) 
//                .Create();

//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(improvement.ApplicationId)).ReturnsAsync(app);

//            // Act
//            OperationResult result = await _improvementService.UpdateAsync(improvement);

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
//            var improvement = _fixture.Build<Improvement>()
//                .With(i => i.ApplicationId, app.Id)
//                .With(i => i.Members, [member])
//                .Create();

//            _improvementRepositoryMock.Setup(r => r.GetByIdAsync(improvement.Id)).ReturnsAsync(improvement);
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

//            // Act
//            OperationResult result = await _improvementService.UpdateAsync(improvement);

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

//            var improvement = _fixture.Build<Improvement>()
//                .With(i => i.StatusImprovement, ImprovementStatus.Open)
//                .With(i => i.ClosedAt, (DateTime?)null)
//                .With(i => i.ApplicationId, app.Id)
//                .With(i => i.Members, [member])
//                .Create();

//            var updatedImprovement = _fixture.Build<Improvement>()
//                .With(i => i.Id, improvement.Id)
//                .With(i => i.StatusImprovement, ImprovementStatus.Closed)
//                .With(i => i.ClosedAt, (DateTime?)null)
//                .With(i => i.ApplicationId, app.Id)
//                .With(i => i.Members, [member])
//                .Create();

//            _improvementRepositoryMock.Setup(r => r.GetByIdAsync(improvement.Id)).ReturnsAsync(improvement);
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

//            // Act
//            OperationResult result = await _improvementService.UpdateAsync(updatedImprovement);

//            // Assert
//            Assert.Equal(OperationStatus.Success, result.Status);
//            Assert.NotNull(improvement.ClosedAt);
//        }

//        [Fact]
//        public async Task UpdateAsyncWhenStatusSetToReabertoShouldClearClosedAt()
//        {
//            // Arrange
//            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
//            var squad = _fixture.Build<Squad>().With(s => s.Members, [member]).Create();
//            var app = _fixture.Build<ApplicationData>().With(a => a.Squads, [squad]).Create();

//            var improvement = _fixture.Build<Improvement>()
//                .With(i => i.StatusImprovement, ImprovementStatus.Closed)
//                .With(i => i.ClosedAt, DateTime.UtcNow)
//                .With(i => i.ApplicationId, app.Id)
//                .With(i => i.Members, [member])
//                .Create();

//            var updatedImprovement = _fixture.Build<Improvement>()
//                .With(i => i.Id, improvement.Id)
//                .With(i => i.StatusImprovement, ImprovementStatus.Reopened)
//                .With(i => i.ClosedAt, improvement.ClosedAt)
//                .With(i => i.ApplicationId, app.Id)
//                .With(i => i.Members, [member])
//                .Create();

//            _improvementRepositoryMock.Setup(r => r.GetByIdAsync(improvement.Id)).ReturnsAsync(improvement);
//            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

//            // Act
//            OperationResult result = await _improvementService.UpdateAsync(updatedImprovement);

//            // Assert
//            Assert.Equal(OperationStatus.Success, result.Status);
//            Assert.Null(improvement.ClosedAt);
//        }

//        [Fact]
//        public async Task DeleteAsyncWhenItemDoesNotExist()
//        {
//            // Arrange
//            int id = _fixture.Create<int>();
//            _improvementRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Improvement?)null);

//            // Act
//            OperationResult result = await _improvementService.DeleteAsync(id);

//            // Assert
//            Assert.Equal(OperationStatus.NotFound, result.Status);
//        }

//        [Fact]
//        public async Task DeleteAsyncWhenSuccessful()
//        {
//            // Arrange
//            Improvement improvement = _fixture.Create<Improvement>();
//            _improvementRepositoryMock.Setup(r => r.GetByIdAsync(improvement.Id)).ReturnsAsync(improvement);

//            // Act
//            OperationResult result = await _improvementService.DeleteAsync(improvement.Id);

//            // Assert
//            Assert.Equal(OperationStatus.Success, result.Status);
//        }
//    }
//}