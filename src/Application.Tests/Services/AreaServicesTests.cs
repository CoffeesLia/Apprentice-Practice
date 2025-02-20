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
            Assert.Equal("Cadastrado com sucesso.", result.Message); // Ajuste a mensagem esperada
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
            Assert.Equal("Existem aplicações associadas a esta área.", result.Message); // Ajuste a mensagem esperada
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnSuccess_WhenNoApplicationsExist()
        {
            // Arrange
            var areaId = 1;
            _areaRepositoryMock.Setup(r => r.VerifyAplicationsExistsAsync(areaId)).ReturnsAsync(false);
            _areaRepositoryMock.Setup(r => r.DeleteAsync(areaId, true)).Returns(Task.CompletedTask);

            // Act
            var result = await _areaService.DeleteAsync(areaId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal("Área deletada com sucesso.", result.Message); // Ajuste a mensagem esperada
        }

    }
}


