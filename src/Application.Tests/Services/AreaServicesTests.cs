using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;
using Stellantis.ProjectName.Application.Validators;
using System.Globalization;
using Stellantis.ProjectName.Application.Models.Filters;
using AutoFixture;
using FluentValidation.Results;
using Stellantis.ProjectName.Application.Interfaces.Services;

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
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _areaRepositoryMock = new Mock<IAreaRepository>();
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var areaValidator = new AreaValidator(localizer);

            _unitOfWorkMock.Setup(u => u.AreaRepository).Returns(_areaRepositoryMock.Object);

            _areaService = new AreaService(_unitOfWorkMock.Object, localizer, areaValidator);
        }


        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameIsNullOrEmptyOrWhitespace()
        {
            // Arrange
            var area = new Area("");
            // Act
            var result = await _areaService.CreateAsync(area);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, AreaResources.NameIsRequired), result.Errors.First());

        }


        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var area = new Area("Eu");


            // Act
            var result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, AreaResources.NameValidateLength, AreaValidator.MinimumLength, AreaValidator.MaximumLength), result.Errors.First());

        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameAlreadyExists()
        {
            // Arrange
            var area = new Area("Test Area");
            var pagedResult = new PagedResult<Area>
            {
                Result = [new Area("Test Area") { Id = 1 }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };

            _areaRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<AreaFilter>())).ReturnsAsync(pagedResult);
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(true);

            // Act
            var result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(AreaResources.AlreadyExists, result.Message);
        }



        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenAreaIsValid()
        {
            // Arrange
            var area = new Area("Test Area");
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);

            // Act
            var result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(ServiceResources.RegisteredSuccessfully, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenNameExceedsMaxLength()
        {
            // Arrange
            var area = new Area(new string('A', 256)); // Name com 256 caracteres

            // Act
            var result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultWhenCalledWithValidFilter()
        {
            // Arrange
            var fixture = new Fixture();
            var filter = fixture.Create<AreaFilter>();
            var pagedResult = fixture.Build<PagedResult<Area>>()
                                     .With(pr => pr.Result, fixture.CreateMany<Area>(2).ToList())
                                     .With(pr => pr.Page, 1)
                                     .With(pr => pr.PageSize, 10)
                                     .With(pr => pr.Total, 2)
                                     .Create();

            _areaRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _areaService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Total);
            Assert.IsType<List<Area>>(result.Result);

        }

        [Fact]
        public async Task DeleteAsyncShouldReturnConflictWhenApplicationsExist()
        {
            // Arrange
            var areaId = 1;
            _areaRepositoryMock.Setup(r => r.VerifyAplicationsExistsAsync(areaId)).ReturnsAsync(true);

            // Act
            var result = await _areaService.DeleteAsync(areaId);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenNoApplicationsExist()
        {
            var areaId = 1;
            var area = new Area("Test Area") { Id = areaId };
            _areaRepositoryMock.Setup(r => r.VerifyAplicationsExistsAsync(areaId)).ReturnsAsync(false);
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(areaId)).ReturnsAsync(area);
            _areaRepositoryMock.Setup(r => r.DeleteAsync(area, true)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            // Act
            var result = await _areaService.DeleteAsync(areaId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }


        [Fact]

     
        public async Task GetItemAsyncShouldReturnCompleteWhenAreaExists()
        {
            // Arrange
            var fixture = new Fixture();
            var area = fixture.Create<Area>();

            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);

            // Act
            var result = await _areaService.GetItemAsync(area.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }


        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenAreaDoesNotExist()
        {
            // Arrange
            var fixture = new Fixture();
            var areaId = fixture.Create<int>();
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(areaId)).ReturnsAsync((Area?)null);

            // Act
            var result = await _areaService.GetItemAsync(areaId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var area = new Area("IN");
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);


            // Act
            var result = await _areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenAreaDoesNotExist()
        {
            // Arrange
            var area = new Area("Non-Existent Area") { Id = 1 };
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync((Area?)null);
       

            // Act
            var result = await _areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(AreaResources.NotFound, result.Message);
        }



        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenNameAlreadyExists()
        {
            // Arrange
            var area = new Area("Existing Name") { Id = 1 };
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(true);
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);

            // Act
            var result = await _areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenAreaIsValid()
        {
            // Arrange
            var area = new Area("Valid Name") { Id = 1 };
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);
            _areaRepositoryMock.Setup(r => r.UpdateAsync(area, true)).Returns(Task.CompletedTask);

            // Act
            var result = await _areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}