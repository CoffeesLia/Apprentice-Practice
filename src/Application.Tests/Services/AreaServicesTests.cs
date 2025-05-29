using System.Globalization;
using AutoFixture;
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
    public class AreaServicesTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAreaRepository> _areaRepositoryMock;
        private readonly AreaService _areaService;

        public AreaServicesTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _areaRepositoryMock = new Mock<IAreaRepository>();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = Helpers.LocalizerFactorHelper.Create();
            AreaValidator areaValidator = new(localizer);

            _unitOfWorkMock.Setup(u => u.AreaRepository).Returns(_areaRepositoryMock.Object);

            _areaService = new AreaService(_unitOfWorkMock.Object, localizer, areaValidator);
        }


        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameIsNullOrEmptyOrWhitespace()
        {
            // Arrange
            Area area = new("");
            // Act
            OperationResult result = await _areaService.CreateAsync(area);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, AreaResources.NameIsRequired), result.Errors.First());

        }


        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            Area area = new("Eu");


            // Act
            OperationResult result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, AreaResources.NameValidateLength, AreaValidator.MinimumLength, AreaValidator.MaximumLength), result.Errors.First());

        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameAlreadyExists()
        {
            // Arrange
            Area area = new("Test Area");
            PagedResult<Area> pagedResult = new()
            {
                Result = [new Area("Test Area") { Id = 1 }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };

            _areaRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<AreaFilter>())).ReturnsAsync(pagedResult);
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(true);

            // Act
            OperationResult result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(AreaResources.AlreadyExists, result.Message);
        }



        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenAreaIsValid()
        {
            // Arrange
            Area area = new("Test Area");
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);

            // Act
            OperationResult result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(ServiceResources.RegisteredSuccessfully, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenNameExceedsMaxLength()
        {
            // Arrange
            Area area = new(new string('A', 256)); // Name com 256 caracteres

            // Act
            OperationResult result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultWhenCalledWithValidFilter()
        {
            // Arrange
            Fixture fixture = new();
            AreaFilter filter = fixture.Create<AreaFilter>();
            PagedResult<Area> pagedResult = fixture.Build<PagedResult<Area>>()
                                     .With(pr => pr.Result, [.. fixture.CreateMany<Area>(2)])
                                     .With(pr => pr.Page, 1)
                                     .With(pr => pr.PageSize, 10)
                                     .With(pr => pr.Total, 2)
                                     .Create();

            _areaRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            PagedResult<Area> result = await _areaService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Total);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnConflictWhenApplicationsExist()
        {
            // Arrange
            int areaId = 1;
            _areaRepositoryMock.Setup(r => r.VerifyAplicationsExistsAsync(areaId)).ReturnsAsync(true);

            // Act
            OperationResult result = await _areaService.DeleteAsync(areaId);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenNoApplicationsExist()
        {
            int areaId = 1;
            Area area = new("Test Area") { Id = areaId };
            _areaRepositoryMock.Setup(r => r.VerifyAplicationsExistsAsync(areaId)).ReturnsAsync(false);
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(areaId)).ReturnsAsync(area);
            _areaRepositoryMock.Setup(r => r.DeleteAsync(area, true)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            // Act
            OperationResult result = await _areaService.DeleteAsync(areaId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }


        [Fact]


        public async Task GetItemAsyncShouldReturnCompleteWhenAreaExists()
        {
            // Arrange
            Fixture fixture = new();
            Area area = fixture.Create<Area>();

            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);

            // Act
            OperationResult result = await _areaService.GetItemAsync(area.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }


        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenAreaDoesNotExist()
        {
            // Arrange
            Fixture fixture = new();
            int areaId = fixture.Create<int>();
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(areaId)).ReturnsAsync((Area?)null);

            // Act
            OperationResult result = await _areaService.GetItemAsync(areaId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            Area area = new("IN");
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);


            // Act
            OperationResult result = await _areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenAreaDoesNotExist()
        {
            // Arrange
            Area area = new("Non-Existent Area") { Id = 1 };
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync((Area?)null);


            // Act
            OperationResult result = await _areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(AreaResources.NotFound, result.Message);
        }



        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenNameAlreadyExists()
        {
            // Arrange
            Area area = new("Existing Name") { Id = 1 };
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(true);
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);

            // Act
            OperationResult result = await _areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenAreaIsValid()
        {
            // Arrange
            Area area = new("Valid Name") { Id = 1 };
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);
            _areaRepositoryMock.Setup(r => r.UpdateAsync(area, true)).Returns(Task.CompletedTask);

            // Act
            OperationResult result = await _areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}