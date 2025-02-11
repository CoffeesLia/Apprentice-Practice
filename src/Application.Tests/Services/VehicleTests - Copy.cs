using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;

namespace Application.Tests.Services
{
    public class VehicleTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IVehicleRepository> _repositoryMock = new();
        private readonly Mock<IPartNumberRepository> _PartNumberRepositoryMock = new();
        private readonly VehicleService _service;

        public VehicleTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddLocalization();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var localizerFactory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();

            _unitOfWorkMock.SetupGet(x => x.VehicleRepository).Returns(_repositoryMock.Object);
            _unitOfWorkMock.SetupGet(x => x.PartNumberRepository).Returns(_PartNumberRepositoryMock.Object);

            _service = new VehicleService(_unitOfWorkMock.Object, localizerFactory);
        }

        [Fact]
        public async Task CreateAsync_Success_WhenThereAreNoPartNumbers()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            vehicle.PartNumbers.Clear();

            _repositoryMock
                .Setup(x => x.VerifyChassiExists(vehicle.Chassi))
                .Returns(false);

            // Act
            var result = await _service.CreateAsync(vehicle);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(ServiceResources.SuccessRegister, result.Message);
        }

        [Fact]
        public async Task CreateAsync_Success_WhenHasPartNumbers()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            var partNumbers = _fixture.CreateMany<VehiclePartNumber>(2).ToArray();
            vehicle.PartNumbers!.Add(partNumbers[0]);
            vehicle.PartNumbers!.Add(partNumbers[1]);

            _repositoryMock
                .Setup(x => x.VerifyChassiExists(vehicle.Chassi))
                .Returns(false);

            // Act
            var result = await _service.CreateAsync(vehicle);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(ServiceResources.SuccessRegister, result.Message);
        }

        [Fact]
        public async Task CreateAsync_Fail_WhenAlreadyExistItemSameChassis()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();

            _repositoryMock.Setup(x => x.VerifyChassiExists(vehicle.Chassi!)).Returns(true);

            // Act
            var result = await _service.CreateAsync(vehicle);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(VehicleResources.AlreadyExistChassis, result.Message);
        }

        [Fact]
        public async Task CreateAsync_Fail_WhenNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_Fail_WhenHasDuplicatePartnumber()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            var partNumber = _fixture.Create<VehiclePartNumber>();
            vehicle.PartNumbers.Add(partNumber);
            vehicle.PartNumbers.Add(partNumber);

            _repositoryMock
                .Setup(x => x.VerifyChassiExists(vehicle.Chassi))
                .Returns(false);

            // Act
            var result = await _service.CreateAsync(vehicle);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(string.Format(VehicleResources.DuplicatePartNumbers, partNumber.PartNumberId), result.Message);
        }


        [Fact]
        public async Task UpdateAsync_Success_ValidVehicle()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            var oldVehicle = _fixture.Create<Vehicle>();

            _unitOfWorkMock
                .Setup(x => x.VehicleRepository.GetFullByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(oldVehicle);

            // Act
            await _service.UpdateAsync(vehicle);

            // Assert
            _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Vehicle>(), true), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Fail_NotFoundVehicle()
        {
            // Arrange
            var addVehicleInputModel = _fixture.Create<Vehicle>();

            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Vehicle)null!);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAsync(addVehicleInputModel));

            // Assert
            Assert.Equal(ServiceResources.NotFound, exception.Message);
        }

        [Fact]
        public async Task GetItemAsync_Success_ReturnVehicle()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();

            _repositoryMock
                .Setup(x => x.GetByIdAsync(vehicle.Id, false))
                .ReturnsAsync(vehicle);

            // Act
            var result = await _service.GetItemAsync(vehicle.Id);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetListAsync_Success_ReturnVehicleList()
        {
            // Arrange
            var vehicleFilter = _fixture.Create<VehicleFilter>();
            var list = _fixture.Create<List<Vehicle>>();
            var paginationVehicle = new PagedResult<Vehicle>()
            {
                Result = list,
                Total = list.Count
            };

            _repositoryMock
                .Setup(x => x.GetListFilter(It.IsAny<VehicleFilter>()))
                .ReturnsAsync(paginationVehicle);

            // Act
            var pageResult = await _service.GetListAsync(vehicleFilter);

            // Assert
            Assert.True(pageResult.Result!.Any());
            Assert.True(pageResult.Total > 0);
        }

        [Fact]
        public async Task DeleteAsync_Success_ValidVehicle()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(vehicle.Id))
                .ReturnsAsync(vehicle);
            vehicle.PartNumbers.Clear();

            // Act
            var result = await _service.DeleteAsync(vehicle.Id);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(ServiceResources.SuccessDelete, result.Message);
        }

        [Fact]
        public async Task DeleteAsync_Fail_NotFoundVehicle()
        {
            // Arrange
            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Vehicle)null!);

            // Act
            var result = await _service.DeleteAsync(_fixture.Create<int>());

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ServiceResources.NotFound, result.Message);
        }

        [Fact]
        public async Task DeleteAsync_Fail_VehicleHasPartNumbers()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            vehicle.PartNumbers.Add(_fixture.Create<VehiclePartNumber>());

            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(vehicle.Id))
                .ReturnsAsync(vehicle);

            // Act
            var result = await _service.DeleteAsync(vehicle.Id);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(VehicleResources.Undeleted, result.Message);
        }
    }
}
