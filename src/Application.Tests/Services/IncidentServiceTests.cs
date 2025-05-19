using Application.Tests.Helpers;
using AutoFixture;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.Validators;
using Stellantis.ProjectName.Domain.Entities;
using System.Globalization;
using Xunit;

namespace Application.Tests.Services
{
    public class IncidentServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IIncidentRepository> _incidentRepositoryMock;
        private readonly Mock<IApplicationDataRepository> _applicationDataRepositoryMock;
        private readonly IncidentService _incidentService;
        private readonly Fixture _fixture;
        private readonly IStringLocalizerFactory _localizerFactory;
        private readonly IncidentValidator _incidentValidator;

        public IncidentServiceTests()
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _incidentRepositoryMock = new Mock<IIncidentRepository>();
            _applicationDataRepositoryMock = new Mock<IApplicationDataRepository>();
            _localizerFactory = LocalizerFactorHelper.Create();
            _incidentValidator = new IncidentValidator(_localizerFactory);

            _unitOfWorkMock.Setup(u => u.IncidentRepository).Returns(_incidentRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(_applicationDataRepositoryMock.Object);

            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _incidentService = new IncidentService(_unitOfWorkMock.Object, _localizerFactory, _incidentValidator);
        }

        [Fact]
        public void CulturePropertyShouldGetAndSetCorrectValue()
        {
            // Arrange
            CultureInfo expectedCulture = new("pt-BR");

            // Act
            IncidentResource.Culture = expectedCulture;
            CultureInfo result = IncidentResource.Culture;

            // Assert
            Assert.Equal(expectedCulture, result);
        }

        [Fact]
        public async Task CreateAsyncWhenValidationFails()
        {
            // Arrange
            Incident incident = _fixture.Build<Incident>()
                .With(i => i.Title, string.Empty)
                .With(i => i.Description, string.Empty)
                .With(i => i.ApplicationId, 0) 

                .Create();

            // Act
            OperationResult result = await _incidentService.CreateAsync(incident);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(IncidentResource.TitleRequired, result.Errors);
            Assert.Contains(IncidentResource.ApplicationRequired, result.Errors);
            Assert.Contains(IncidentResource.DescriptionRequired, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncWhenApplicationNotFound()
        {
            // Arrange
            Incident incident = _fixture.Create<Incident>();
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync((ApplicationData?)null);

            // Act
            OperationResult result = await _incidentService.CreateAsync(incident);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task CreateAsyncWhenInvalidMembers()
        {
            // Arrange
            var incident = _fixture.Create<Incident>();
            var app = _fixture.Build<ApplicationData>()
                .With(a => a.Id, incident.ApplicationId)
                .With(a => a.Squads, new List<Squad>()) // Nenhum membro válido
                .Create();

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync(app);

            // Act
            OperationResult result = await _incidentService.CreateAsync(incident);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains(IncidentResource.InvalidMembers, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncWhenSuccessful()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
            var squad = _fixture.Build<Squad>().With(s => s.Members, new List<Member> { member }).Create();
            var app = _fixture.Build<ApplicationData>().With(a => a.Squads, new List<Squad> { squad }).Create();
            var incident = _fixture.Build<Incident>()
                .With(i => i.ApplicationId, app.Id)
                .With(i => i.Members, new List<Member> { member })
                .Create();

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

            // Act
            OperationResult result = await _incidentService.CreateAsync(incident);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            IncidentFilter filter = _fixture.Create<IncidentFilter>();
            PagedResult<Incident> pagedResult = _fixture.Create<PagedResult<Incident>>();
            _incidentRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            PagedResult<Incident> result = await _incidentService.GetListAsync(filter);

            // Assert
            Assert.Equal(pagedResult, result);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Incident?)null);

            // Act
            OperationResult result = await _incidentService.GetItemAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemExists()
        {
            // Arrange
            Incident incident = _fixture.Create<Incident>();
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);

            // Act
            OperationResult result = await _incidentService.GetItemAsync(incident.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenValidationFails()
        {
            // Arrange
            Incident incident = _fixture.Build<Incident>()
                .With(i => i.Title, string.Empty)
                .With(i => i.Description, string.Empty)
                .Create();

            // Act
            OperationResult result = await _incidentService.UpdateAsync(incident);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenIncidentNotFound()
        {
            // Arrange
            Incident incident = _fixture.Create<Incident>();
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync((Incident?)null);

            // Act
            OperationResult result = await _incidentService.UpdateAsync(incident);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenApplicationNotFound()
        {
            // Arrange
            Incident incident = _fixture.Create<Incident>();
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync((ApplicationData?)null);

            // Act
            OperationResult result = await _incidentService.UpdateAsync(incident);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenInvalidMembers()
        {
            // Arrange
            var incident = _fixture.Create<Incident>();
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);

            var app = _fixture.Build<ApplicationData>()
                .With(a => a.Id, incident.ApplicationId)
                .With(a => a.Squads, new List<Squad>()) // Nenhum membro válido
                .Create();

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync(app);

            // Act
            OperationResult result = await _incidentService.UpdateAsync(incident);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenSuccessful()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
            var squad = _fixture.Build<Squad>().With(s => s.Members, new List<Member> { member }).Create();
            var app = _fixture.Build<ApplicationData>().With(a => a.Squads, new List<Squad> { squad }).Create();
            var incident = _fixture.Build<Incident>()
                .With(i => i.ApplicationId, app.Id)
                .With(i => i.Members, new List<Member> { member })
                .Create();

            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

            // Act
            OperationResult result = await _incidentService.UpdateAsync(incident);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenStatusSetToFechadoShouldSetClosedAt()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
            var squad = _fixture.Build<Squad>().With(s => s.Members, new List<Member> { member }).Create();
            var app = _fixture.Build<ApplicationData>().With(a => a.Squads, new List<Squad> { squad }).Create();

            var incident = _fixture.Build<Incident>()
                .With(i => i.Status, IncidentStatus.Aberto)
                .With(i => i.ClosedAt, (DateTime?)null)
                .With(i => i.ApplicationId, app.Id)
                .With(i => i.Members, new List<Member> { member })
                .Create();

            var updatedIncident = _fixture.Build<Incident>()
                .With(i => i.Id, incident.Id)
                .With(i => i.Status, IncidentStatus.Fechado)
                .With(i => i.ClosedAt, (DateTime?)null)
                .With(i => i.ApplicationId, app.Id)
                .With(i => i.Members, new List<Member> { member })
                .Create();

            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

            // Act
            OperationResult result = await _incidentService.UpdateAsync(updatedIncident);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.NotNull(incident.ClosedAt);
        }

        [Fact]
        public async Task UpdateAsyncWhenStatusSetToReabertoShouldClearClosedAt()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
            var squad = _fixture.Build<Squad>().With(s => s.Members, new List<Member> { member }).Create();
            var app = _fixture.Build<ApplicationData>().With(a => a.Squads, new List<Squad> { squad }).Create();

            var incident = _fixture.Build<Incident>()
                .With(i => i.Status, IncidentStatus.Fechado)
                .With(i => i.ClosedAt, DateTime.UtcNow)
                .With(i => i.ApplicationId, app.Id)
                .With(i => i.Members, new List<Member> { member })
                .Create();

            var updatedIncident = _fixture.Build<Incident>()
                .With(i => i.Id, incident.Id)
                .With(i => i.Status, IncidentStatus.Reaberto)
                .With(i => i.ClosedAt, incident.ClosedAt)
                .With(i => i.ApplicationId, app.Id)
                .With(i => i.Members, new List<Member> { member })
                .Create();

            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

            // Act
            OperationResult result = await _incidentService.UpdateAsync(updatedIncident);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Null(incident.ClosedAt);
        }

        [Fact]
        public async Task DeleteAsyncWhenItemDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Incident?)null);

            // Act
            OperationResult result = await _incidentService.DeleteAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenSuccessful()
        {
            // Arrange
            Incident incident = _fixture.Create<Incident>();
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);

            // Act
            OperationResult result = await _incidentService.DeleteAsync(incident.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}