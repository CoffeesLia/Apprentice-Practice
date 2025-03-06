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
        public async Task CreateAsync_ShouldReturnInvalidData_WhenValidationFails()
        {
            // Arrange
            var area = new Area("Eu");


            // Act
            var result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnConflict_WhenNameAlreadyExists()
        {
            // Arrange
            var area = new Area("Test Area");
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(true);

            // Act
            var result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnSuccess_WhenAreaIsValid()
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
        public async Task CreateAsync_ShouldReturnInvalidData_WhenNameExceedsMaxLength()
        {
            // Arrange
            var area = new Area(new string('A', 256)); // Nome com 256 caracteres

            // Act
            var result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task GetListAsync_ShouldReturnPagedResult_WhenCalledWithValidFilter()
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
        public async Task DeleteAsync_ShouldReturnConflict_WhenApplicationsExist()
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
        public async Task DeleteAsync_ShouldReturnSuccess_WhenNoApplicationsExist()
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

        public async Task GetItemAsync_ShouldReturnComplete_WhenAreaExists()

        {

            // Arrange

            var fixture = new Fixture();

            var area = fixture.Create<Area>();

            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);

            // Act

            var result = await _areaService.GetItemAsync(area.Id);

            // Assert
<<<<<<< HEAD

            Assert.IsType<Area>(result);
=======
            Assert.IsType<Area>(result);
           

>>>>>>> teste
        }


        [Fact]
        public async Task GetItemAsync_ShouldReturnNotFound_WhenAreaDoesNotExist()
        {
            // Arrange
            var fixture = new Fixture();
            var areaId = fixture.Create<int>();
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(areaId)).ReturnsAsync((Area)null);

            // Act
            var result = await _areaService.GetItemAsync(areaId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnInvalidData_WhenValidationFails()
        {
            // Arrange
            var area = new Area("IN");
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var areaValidator = new AreaValidator(localizer);
            var areaService = new AreaService(_unitOfWorkMock.Object, localizer, areaValidator);

            // Act
<<<<<<< HEAD
            var result = await ((Stellantis.ProjectName.Application.Interfaces.Services.IEntityServiceBase<Area>)areaService).UpdateAsync(area);
=======
            var result = await areaService.UpdateAsync(area);
>>>>>>> teste

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnConflict_WhenNameAlreadyExists()
        {
            // Arrange
            var area = new Area("Existing Name");
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(true);
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<Area>>();
            validatorMock.Setup(v => v.ValidateAsync(area, It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var areaService = new AreaService(_unitOfWorkMock.Object, localizer, validatorMock.Object);

            // Act
<<<<<<< HEAD
            var result = await ((Stellantis.ProjectName.Application.Interfaces.Services.IEntityServiceBase<Area>)areaService).UpdateAsync(area);
=======
            var result = await areaService.UpdateAsync(area);
>>>>>>> teste

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnSuccess_WhenAreaIsValid()
        {
            // Arrange
            var area = new Area("Valid Name") { Id = 1 };
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);
            _areaRepositoryMock.Setup(r => r.UpdateAsync(area, true)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<Area>>();
            validatorMock.Setup(v => v.ValidateAsync(area, It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var areaService = new AreaService(_unitOfWorkMock.Object, localizer, validatorMock.Object);

            // Act
<<<<<<< HEAD
            var result = await ((Stellantis.ProjectName.Application.Interfaces.Services.IEntityServiceBase<Area>)areaService).UpdateAsync(area);
=======
            var result = await areaService.UpdateAsync(area);
>>>>>>> teste

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}