using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class VehicleRepositoryTests
    {
        private readonly IFixture _fixture;
        private readonly Context _context;
        private readonly VehicleRepository _repository;

        public VehicleRepositoryTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new VehicleRepository(_context);
        }

        [Fact]
        public async Task GetListAsync_ByChassis()
        {
            // Arrange
            var count = 10;
            var filter = new VehicleFilter
            {
                Chassis = _fixture.Create<string>(),
                Page = 1,
                RowsPerPage = 10
            };
            var vehicles = _fixture
                .Build<Vehicle>()
                .With(x => x.Chassi, filter.Chassis)
                .CreateMany<Vehicle>(count);
            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(count, result.Total);
        }

        [Fact]
        public async Task RemovePartnumbers_WhenNotReturnPartnumbers()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            _context.Vehicles.Add(vehicle);
            var partNumbers = _fixture
                .Build<VehiclePartNumber>()
                .With(x => x.VehicleId, vehicle.Id)
                .CreateMany(10);
            await _context.VehiclePartNumbers.AddRangeAsync(partNumbers);
            await _context.SaveChangesAsync();

            // Act
            _repository.RemovePartnumbers(vehicle.PartNumbers);
            var result = _context.VehiclePartNumbers.Where(x => x.VehicleId == vehicle.Id);

            // Assert
            Assert.Equal(0, await result.CountAsync());
        }

        [Fact]
        public async Task VerifyChassiExists_ReturnTrue_WhenChassiExists()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.VerifyChassiExistsAsync(vehicle.Chassi);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyChassiExists_ReturnFalse_WhenChassiDoesNotExist()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            await _repository.CreateAsync(vehicle);

            // Act
            var result = await _repository.VerifyChassiExistsAsync(vehicle.Chassi + "_");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetFullByIdAsync_ReturnVehicleWithPartNumbers()
        {
            // Arrange
            var vehicle = _fixture.Create<Vehicle>();
            await _context.Vehicles.AddAsync(vehicle);
            var partNumbers = _fixture
                .Build<VehiclePartNumber>()
                .With(x => x.VehicleId, vehicle.Id)
                .CreateMany(10);
            await _context.VehiclePartNumbers.AddRangeAsync(partNumbers);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFullByIdAsync(vehicle.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(vehicle.Id, result.Id);
            Assert.Equal(vehicle.Chassi, result.Chassi);
            Assert.Equal(vehicle.PartNumbers, result.PartNumbers);
        }
    }
}
