using System.Globalization;
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
            CultureInfo expectedCulture = new("pt-BR");
            IncidentResource.Culture = expectedCulture;
            CultureInfo result = IncidentResource.Culture;
            Assert.Equal(expectedCulture, result);
        }

        [Fact]
        public async Task CreateAsyncWhenValidationFails()
        {
            var incident = new Incident
            {
                Title = string.Empty,
                Description = string.Empty,
                ApplicationId = 0,
                Application = new ApplicationData("App") { Id = 0, ProductOwner = "PO", ConfigurationItem = "CI" },
                Members = new List<Member>()
            };

            OperationResult result = await _incidentService.CreateAsync(incident);

            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(IncidentResource.TitleRequired, result.Errors);
            Assert.Contains(IncidentResource.ApplicationRequired, result.Errors);
            Assert.Contains(IncidentResource.DescriptionRequired, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncWhenApplicationNotFound()
        {
            var incident = new Incident
            {
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 123,
                Application = new ApplicationData("App") { Id = 123, ProductOwner = "PO", ConfigurationItem = "CI" },
                Members = new List<Member>()
            };
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync((ApplicationData?)null);

            OperationResult result = await _incidentService.CreateAsync(incident);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task CreateAsyncWhenInvalidMembers()
        {
            var incident = new Incident
            {
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Application = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" },
                Members = new List<Member> { new Member { Id = 99, Name = "Inválido", Role = "Dev", Cost = 1, Email = "inv@x.com", SquadId = 1, Squad = new Squad() } }
            };
            var app = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" };
            // Squads é somente leitura, então adicionamos via Add
            // app.Squads = new List<Squad>(); // ERRADO
            // Não adiciona nenhum squad, lista fica vazia

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync(app);

            OperationResult result = await _incidentService.CreateAsync(incident);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains(IncidentResource.InvalidMembers, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncWhenSuccessful()
        {
            var member = new Member { Id = 1, Name = "M", Role = "Dev", Cost = 1, Email = "m@x.com", SquadId = 1, Squad = new Squad() };
            var squad = new Squad { Id = 1, Name = "S" };
            squad.Members.Add(member); // Adiciona membro na coleção somente leitura

            var app = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" };
            app.Squads.Add(squad); // Adiciona squad na coleção somente leitura

            var incident = new Incident
            {
                Title = "Teste",
                Description = "Desc",
                ApplicationId = app.Id,
                Application = app,
                Members = new List<Member> { member }
            };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

            OperationResult result = await _incidentService.CreateAsync(incident);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            IncidentFilter filter = _fixture.Create<IncidentFilter>();
            PagedResult<Incident> pagedResult = _fixture.Create<PagedResult<Incident>>();
            _incidentRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            PagedResult<Incident> result = await _incidentService.GetListAsync(filter);

            Assert.Equal(pagedResult, result);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemDoesNotExist()
        {
            int id = _fixture.Create<int>();
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Incident?)null);

            OperationResult result = await _incidentService.GetItemAsync(id);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemExists()
        {
            var incident = new Incident
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Application = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" },
                Members = new List<Member>()
            };
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);

            OperationResult result = await _incidentService.GetItemAsync(incident.Id);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenValidationFails()
        {
            var incident = new Incident
            {
                Id = 1,
                Title = string.Empty,
                Description = string.Empty,
                ApplicationId = 1,
                Application = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" },
                Members = new List<Member>()
            };

            OperationResult result = await _incidentService.UpdateAsync(incident);

            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenIncidentNotFound()
        {
            var incident = new Incident
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Application = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" },
                Members = new List<Member>()
            };
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync((Incident?)null);

            OperationResult result = await _incidentService.UpdateAsync(incident);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenApplicationNotFound()
        {
            var incident = new Incident
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Application = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" },
                Members = new List<Member>()
            };
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync((ApplicationData?)null);

            OperationResult result = await _incidentService.UpdateAsync(incident);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenInvalidMembers()
        {
            var incident = new Incident
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Application = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" },
                Members = new List<Member> { new Member { Id = 99, Name = "Inválido", Role = "Dev", Cost = 1, Email = "inv@x.com", SquadId = 1, Squad = new Squad() } }
            };
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);

            var app = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" };
            // Não adiciona nenhum squad, lista fica vazia

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync(app);

            OperationResult result = await _incidentService.UpdateAsync(incident);

            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenSuccessful()
        {
            var member = new Member { Id = 1, Name = "M", Role = "Dev", Cost = 1, Email = "m@x.com", SquadId = 1, Squad = new Squad() };
            var squad = new Squad { Id = 1, Name = "S" };
            squad.Members.Add(member);

            var app = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" };
            app.Squads.Add(squad);

            var incident = new Incident
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = app.Id,
                Application = app,
                Members = new List<Member> { member }
            };

            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

            OperationResult result = await _incidentService.UpdateAsync(incident);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenStatusSetToFechadoShouldSetClosedAt()
        {
            var member = new Member { Id = 1, Name = "M", Role = "Dev", Cost = 1, Email = "m@x.com", SquadId = 1, Squad = new Squad() };
            var squad = new Squad { Id = 1, Name = "S" };
            squad.Members.Add(member);

            var app = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" };
            app.Squads.Add(squad);

            var incident = new Incident
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = app.Id,
                Application = app,
                Members = new List<Member> { member },
                Status = IncidentStatus.Open,
                ClosedAt = null
            };

            var updatedIncident = new Incident
            {
                Id = incident.Id,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = app.Id,
                Application = app,
                Members = new List<Member> { member },
                Status = IncidentStatus.Closed,
                ClosedAt = null
            };

            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

            OperationResult result = await _incidentService.UpdateAsync(updatedIncident);

            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.NotNull(incident.ClosedAt);
        }

        [Fact]
        public async Task UpdateAsyncWhenStatusSetToReabertoShouldClearClosedAt()
        {
            var member = new Member { Id = 1, Name = "M", Role = "Dev", Cost = 1, Email = "m@x.com", SquadId = 1, Squad = new Squad() };
            var squad = new Squad { Id = 1, Name = "S" };
            squad.Members.Add(member);

            var app = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" };
            app.Squads.Add(squad);

            var incident = new Incident
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = app.Id,
                Application = app,
                Members = new List<Member> { member },
                Status = IncidentStatus.Closed,
                ClosedAt = DateTime.UtcNow
            };

            var updatedIncident = new Incident
            {
                Id = incident.Id,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = app.Id,
                Application = app,
                Members = new List<Member> { member },
                Status = IncidentStatus.Reopened,
                ClosedAt = incident.ClosedAt
            };

            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(app.Id)).ReturnsAsync(app);

            OperationResult result = await _incidentService.UpdateAsync(updatedIncident);

            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Null(incident.ClosedAt);
        }

        [Fact]
        public async Task DeleteAsyncWhenItemDoesNotExist()
        {
            int id = _fixture.Create<int>();
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Incident?)null);

            OperationResult result = await _incidentService.DeleteAsync(id);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenSuccessful()
        {
            var incident = new Incident
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Application = new ApplicationData("App") { Id = 1, ProductOwner = "PO", ConfigurationItem = "CI" },
                Members = new List<Member>()
            };
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);

            OperationResult result = await _incidentService.DeleteAsync(incident.Id);

            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}