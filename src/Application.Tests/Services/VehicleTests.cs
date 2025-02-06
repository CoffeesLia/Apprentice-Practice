using Application.Tests.Helpers;
using AutoFixture;
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
    public class VehicleServiceTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IVehicleRepository> _repositoryMock = new();
        private readonly VehicleService _service;

        public VehicleServiceTests()
        {
            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            _unitOfWorkMock.SetupGet(x => x.VehicleRepository).Returns(_repositoryMock.Object);
            _service = new VehicleService(_unitOfWorkMock.Object, localizerFactory);
        }

        /// <summary>
        /// Given a Vehicle with duplicate part numbers,
        /// when CreateAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task CreateAsync_Fail_WhenHasDuplicatePartNumber()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            var partNumber = _fixture.Create<VehiclePartNumber>();
            vehicle.PartNumbers!.Add(partNumber);
            vehicle.PartNumbers.Add(partNumber);

            _repositoryMock
                .Setup(x => x.VerifyChassiExistsAsync(vehicle.Chassi))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(vehicle);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(string.Format(VehicleResources.DuplicatePartNumbers, partNumber.PartNumberId), result.Message);
        }

        /// <summary>
        /// Given a Vehicle with an existing code,
        /// when CreateAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task CreateAsync_Fail_WhenAlreadyExistItemSameCode()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();

            _repositoryMock
                .Setup(x => x.VerifyChassiExistsAsync(vehicle.Chassi))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CreateAsync(vehicle);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(VehicleResources.AlreadyExistChassis, result.Message);
        }

        /// <summary>
        /// Given a Vehicle with part numbers,
        /// when CreateAsync is called,
        /// then it should succeed.
        /// </summary>
        [Fact]
        public async Task CreateAsync_Success_WhenHasPartNumbers()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            var partNumbers = _fixture.CreateMany<VehiclePartNumber>(2).ToArray();
            vehicle.PartNumbers!.Add(partNumbers[0]);
            vehicle.PartNumbers!.Add(partNumbers[1]);

            _repositoryMock
                .Setup(x => x.VerifyChassiExistsAsync(vehicle.Chassi))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(vehicle);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.RegisteredSuccessfully, result.Message);
        }

        /// <summary>
        /// Given a Vehicle without part numbers,
        /// when CreateAsync is called,
        /// then it should succeed.
        /// </summary>
        [Fact]
        public async Task CreateAsync_Success_WhenThereAreNoPartNumbers()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();

            _repositoryMock
                .Setup(x => x.VerifyChassiExistsAsync(vehicle.Chassi))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(vehicle);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.RegisteredSuccessfully, result.Message);
        }

        /// <summary>
        /// Given a Vehicle with part numbers,
        /// when DeleteAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_Fail_WhenHasPartNumbers()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            vehicle.PartNumbers.Add(new Fixture().Create<VehiclePartNumber>());

            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(vehicle.Id))
                .ReturnsAsync(vehicle);

            // Act
            var result = await _service.DeleteAsync(vehicle.Id);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(VehicleResources.Undeleted, result.Message);
        }

        /// <summary>
        /// Given a Vehicle that does not exist,
        /// when DeleteAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_Fail_WhenNotFound()
        {
            // Act
            var result = await _service.DeleteAsync(_fixture.Create<int>());

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(GeneralResources.NotFound, result.Message);
        }

        /// <summary>
        /// Given a valid Vehicle,
        /// when DeleteAsync is called,
        /// then it should succeed.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_Success()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            vehicle.PartNumbers.Clear();
            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(vehicle.Id))
                .ReturnsAsync(vehicle);

            // Act
            var result = await _service.DeleteAsync(vehicle.Id);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.DeletedSuccessfully, result.Message);
        }

        /// <summary>
        /// Given a Vehicle ID,
        /// when GetItemAsync is called,
        /// then it should return the vehicle.
        /// </summary>
        [Fact]
        public async Task GetItemAsync_Success()
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
            Assert.Equal(vehicle.Id, result.Id);
            Assert.Equal(vehicle.Chassi, result.Chassi);
        }

        /// <summary>
        /// Given a Vehicle filter,
        /// when GetListAsync is called,
        /// then it should return a list of Vehicles.
        /// </summary>
        [Fact]
        public async Task GetListAsync_Success()
        {
            // Arrange
            var filter = _fixture.Create<VehicleFilter>();
            var pagedResult = _fixture.Create<PagedResult<Vehicle>>();

            _repositoryMock
                .Setup(x => x.GetListAsync(It.IsAny<VehicleFilter>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _service.GetListAsync(filter);

            // Assert
            Assert.True(result.Result!.Any());
            Assert.True(result.Total > 0);
        }

        /// <summary>
        /// Given a Vehicle with duplicate part numbers,
        /// when UpdateAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_Fail_WhenHasDuplicatePartNumber()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            var partNumber = _fixture.Create<VehiclePartNumber>();
            vehicle.PartNumbers.Add(partNumber);
            vehicle.PartNumbers.Add(partNumber);

            // Act
            var result = await _service.UpdateAsync(vehicle);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(string.Format(VehicleResources.DuplicatePartNumbers, partNumber.PartNumberId), result.Message);
        }

        /// <summary>
        /// Given a Vehicle that does not exist,
        /// when UpdateAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_Fail_WhenNotFound()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();

            // Act
            var result = await _service.UpdateAsync(vehicle);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(GeneralResources.NotFound, result.Message);
        }

        /// <summary>
        /// Given a Vehicle with part numbers,
        /// when UpdateAsync is called,
        /// then it should succeed.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_Success_WhenHasPartNumbers()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            var partNumbers = _fixture.CreateMany<VehiclePartNumber>(2).ToArray();
            vehicle.PartNumbers!.Add(partNumbers[0]);
            vehicle.PartNumbers!.Add(partNumbers[1]);

            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(vehicle.Id))
                .ReturnsAsync(vehicle);

            // Act
            var result = await _service.UpdateAsync(vehicle);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.UpdatedSuccessfully, result.Message);
        }

        /// <summary>
        /// Given a Vehicle without part numbers,
        /// when UpdateAsync is called,
        /// then it should succeed.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_Success_WhenThereAreNoPartNumbers()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();

            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(vehicle.Id))
                .ReturnsAsync(vehicle);

            // Act
            var result = await _service.UpdateAsync(vehicle);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.UpdatedSuccessfully, result.Message);
        }
    }
}
