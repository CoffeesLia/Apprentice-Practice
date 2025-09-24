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
    public class IncidentServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IIncidentRepository> _incidentRepositoryMock;
        private readonly Mock<IApplicationDataRepository> _applicationDataRepositoryMock;
        private readonly Mock<IMemberRepository> _memberRepositoryMock;
        private readonly Mock<IStringLocalizer<NotificationResources>> _notificationLocalizerMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly IStringLocalizerFactory _localizerFactory;
        private readonly IncidentValidator _incidentValidator;
        private readonly IncidentService _incidentService;
        private readonly Fixture _fixture;

        public IncidentServiceTests()
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _incidentRepositoryMock = new Mock<IIncidentRepository>();
            _applicationDataRepositoryMock = new Mock<IApplicationDataRepository>();
            _memberRepositoryMock = new Mock<IMemberRepository>();
            _notificationLocalizerMock = new Mock<IStringLocalizer<NotificationResources>>();
            _notificationServiceMock = new Mock<INotificationService>();
            _localizerFactory = LocalizerFactorHelper.Create();
            _incidentValidator = new IncidentValidator(_localizerFactory);

            _unitOfWorkMock.Setup(u => u.IncidentRepository).Returns(_incidentRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(_applicationDataRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(_memberRepositoryMock.Object);

            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _incidentService = new IncidentService(
                _unitOfWorkMock.Object,
                _localizerFactory,
                _notificationLocalizerMock.Object,
                _incidentValidator,
                _notificationServiceMock.Object
            );
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
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            var incident = new Incident
            {
                Title = "",
                Description = "",
                ApplicationId = 0,
                Members = []
            };

            var result = await _incidentService.CreateAsync(incident);

            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(IncidentResource.TitleRequired, result.Errors);
            Assert.Contains(IncidentResource.ApplicationRequired, result.Errors);
            Assert.Contains(IncidentResource.DescriptionRequired, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnNotFoundWhenApplicationDoesNotExist()
        {
            var incident = new Incident
            {
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Members = []
            };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync((ApplicationData?)null);

            var result = await _incidentService.CreateAsync(incident);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenMemberNotInSquad()
        {
            var member = new Member { Id = 1, Name = "M", Role = "Dev", Cost = 1, Email = "m@x.com", SquadId = 99 };
            var incident = new Incident
            {
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Members = [member]
            };
            var app = new ApplicationData("App") { Id = 1, SquadId = 1, ProductOwner = "PO" };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync(app);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = [member] });

            var result = await _incidentService.CreateAsync(incident);

            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenValid()
        {
            var member = new Member { Id = 1, Name = "M", Role = "Dev", Cost = 1, Email = "m@x.com", SquadId = 1 };
            var incident = new Incident
            {
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Members = [member]
            };
            var app = new ApplicationData("App") { Id = 1, SquadId = 1, ProductOwner = "PO" };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync(app);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = [member] });
            _incidentRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Incident>(), true)).Returns(Task.CompletedTask);

            var result = await _incidentService.CreateAsync(incident);

            Assert.Equal(OperationStatus.Success, result.Status);
            _notificationServiceMock.Verify(n => n.NotifyIncidentCreatedAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            var filter = _fixture.Create<IncidentFilter>();
            var pagedResult = _fixture.Create<PagedResult<Incident>>();
            _incidentRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            var result = await _incidentService.GetListAsync(filter);

            Assert.Equal(pagedResult, result);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenItemDoesNotExist()
        {
            int id = _fixture.Create<int>();
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Incident?)null);

            var result = await _incidentService.GetItemAsync(id);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnSuccessWhenItemExists()
        {
            var incident = new Incident
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Members = []
            };
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);

            var result = await _incidentService.GetItemAsync(incident.Id);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenIncidentDoesNotExist()
        {
            var incident = new Incident { Id = 1, Title = "Teste", Description = "Desc", ApplicationId = 1 };

            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync((Incident?)null);

            var result = await _incidentService.UpdateAsync(incident);

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
            var existing = new Incident
            {
                Id = 1,
                Title = "Old",
                Description = "Old",
                ApplicationId = 1,
                Application = app,
                Members = []
            };
            var incident = new Incident
            {
                Id = 1,
                Title = "",
                Description = "",
                ApplicationId = 1,
                Application = app,
                Members = []
            };

            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(existing);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = [] });

            var result = await _incidentService.UpdateAsync(incident);

            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenApplicationDoesNotExist()
        {
            var app = new ApplicationData("App") { Id = 1, ProductOwner = "PO", SquadId = 1 };
            var existing = new Incident
            {
                Id = 1,
                Title = "Old",
                Description = "Old",
                ApplicationId = 1,
                Application = app,
                Members = []
            };
            var incident = new Incident
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Application = app,
                Members = []
            };

            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(existing);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync((ApplicationData?)null);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = [] });

            var result = await _incidentService.UpdateAsync(incident);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenMemberNotInSquad()
        {
            var member = new Member { Id = 1, Name = "M", Role = "Dev", Cost = 1, Email = "m@x.com", SquadId = 99 };
            var existing = new Incident { Id = 1, Title = "Old", Description = "Old", ApplicationId = 1, Members = [] };
            var incident = new Incident { Id = 1, Title = "Teste", Description = "Desc", ApplicationId = 1, Members = [member] };
            var app = new ApplicationData("App") { Id = 1, SquadId = 1, ProductOwner = "PO" };

            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(existing);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync(app);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = [member] });

            var result = await _incidentService.UpdateAsync(incident);

            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenValid()
        {
            var member = new Member { Id = 1, Name = "M", Role = "Dev", Cost = 1, Email = "m@x.com", SquadId = 1 };
            var existing = new Incident { Id = 1, Title = "Old", Description = "Old", ApplicationId = 1, Members = [] };
            var incident = new Incident { Id = 1, Title = "Teste", Description = "Desc", ApplicationId = 1, Members = [member] };
            var app = new ApplicationData("App") { Id = 1, SquadId = 1, ProductOwner = "PO" };

            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(existing);
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(incident.ApplicationId)).ReturnsAsync(app);
            _memberRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Member, bool>>>(), null, null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Member> { Result = [member] });
            _incidentRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Incident>(), true)).Returns(Task.CompletedTask);

            var result = await _incidentService.UpdateAsync(incident);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenItemDoesNotExist()
        {
            int id = _fixture.Create<int>();
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Incident?)null);

            var result = await _incidentService.DeleteAsync(id);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenSuccessful()
        {
            var incident = new Incident
            {
                Id = 1,
                Title = "Teste",
                Description = "Desc",
                ApplicationId = 1,
                Members = []
            };
            _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incident.Id)).ReturnsAsync(incident);
            _incidentRepositoryMock.Setup(r => r.DeleteAsync(incident, true)).Returns(Task.CompletedTask);

            var result = await _incidentService.DeleteAsync(incident.Id);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public void IncidentServiceInvalidMembersResourceReturnsExpectedString()
        {
            // Arrange
            var expected = IncidentResource.InvalidMembers;

            // Act
            var actual = IncidentResource.InvalidMembers;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IncidentResourceStatusRequiredPropertyReturnsExpectedString()
        {
            // Arrange
            var expected = IncidentResource.StatusRequired;

            // Act
            var actual = IncidentResource.StatusRequired;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IncidentResourceConstructorCanBeInvoked()
        {
            // Act
            var resource = Activator.CreateInstance(typeof(IncidentResource), true);

            // Assert
            Assert.NotNull(resource);
        }
    }
}