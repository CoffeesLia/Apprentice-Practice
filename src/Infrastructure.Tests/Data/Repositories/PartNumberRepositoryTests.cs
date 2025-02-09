using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Domain.Enums;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class PartNumberRepositoryTests
    {
        private readonly Context _context;
        private readonly PartNumberRepository _repository;
        private readonly Fixture _fixture;

        public PartNumberRepositoryTests()
        {
            _fixture = new Fixture();
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;

            _context = new Context(options);
            _repository = new PartNumberRepository(_context);
        }

        [Fact]
        public async Task GetListAsync_ByCode()
        {
            // Arrange
            var filter = new PartNumberFilter
            {
                Page = 1,
                PageSize = 10,
                Code = _fixture.Create<string>()
            };
            const int Count = 10;
            var partNumbers = _fixture
                .Build<PartNumber>()
                .With(x => x.Code, filter.Code)
                .CreateMany<PartNumber>(Count);
            await _context.PartNumbers.AddRangeAsync(partNumbers);
            partNumbers = _fixture
                .Build<PartNumber>()
                .With(x => x.Code, _fixture.Create<string>())
                .CreateMany<PartNumber>(Count);
            await _context.PartNumbers.AddRangeAsync(partNumbers);
            await _context.SaveChangesAsync();

            // Act
            var list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
        }

        [Fact]
        public async Task GetListAsync_ByDescription()
        {
            // Arrange
            var filter = new PartNumberFilter
            {
                Page = 1,
                PageSize = 10,
                Description = _fixture.Create<string>()
            };
            const int Count = 10;
            var partNumbers = _fixture
                .Build<PartNumber>()
                .FromFactory(() => new PartNumber(_fixture.Create<string>(), filter.Description, PartNumberType.External))
                .CreateMany<PartNumber>(Count);
            await _context.PartNumbers.AddRangeAsync(partNumbers);
            partNumbers = _fixture
                .Build<PartNumber>()
                .FromFactory(() => new PartNumber(_fixture.Create<string>(), _fixture.Create<string>(), PartNumberType.External))
                .CreateMany<PartNumber>(Count);
            await _context.PartNumbers.AddRangeAsync(partNumbers);
            await _context.SaveChangesAsync();

            // Act
            var list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
        }


        [Fact]
        public async Task GetListAsync_ByType()
        {
            // Arrange
            var filter = new PartNumberFilter
            {
                Page = 1,
                PageSize = 10,
                Type = PartNumberType.External
            };
            const int Count = 10;
            var partNumbers = _fixture
                .Build<PartNumber>()
                .FromFactory(() => new PartNumber(_fixture.Create<string>(), _fixture.Create<string>(), PartNumberType.External))
                .CreateMany<PartNumber>(Count);
            await _context.PartNumbers.AddRangeAsync(partNumbers);
            partNumbers = _fixture
                .Build<PartNumber>()
                .FromFactory(() => new PartNumber(_fixture.Create<string>(), _fixture.Create<string>(), PartNumberType.Internal))
                .CreateMany<PartNumber>(Count);
            await _context.PartNumbers.AddRangeAsync(partNumbers);
            await _context.SaveChangesAsync();

            // Act
            var list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
        }

        [Fact]
        public void VerifyCodeExists_ReturnTrue_WhenCodeExists()
        {
            // Arrange
            var partNumber = _fixture.Create<PartNumber>();
            _context.PartNumbers.Add(partNumber);
            _context.SaveChanges();

            // Act
            var result = _repository.VerifyCodeExists(partNumber.Code);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyCodeExists_ReturnTrue_WhenCodeNoExists()
        {
            // Arrange
            var partNumber = _fixture.Create<PartNumber>();
            _context.PartNumbers.Add(partNumber);
            _context.SaveChanges();

            // Act
            var result = _repository.VerifyCodeExists(partNumber.Code + "_");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetFullByIdAsync_Success()
        {
            // Arrange
            var partNumber = _fixture.Create<PartNumber>();
            var supplier = _fixture.Create<Supplier>();
            var vehicle = _fixture.Create<Vehicle>();
            var partNumberSupplier = new PartNumberSupplier(partNumber.Id, supplier.Id, _fixture.Create<decimal>());
            var vehiclePartNumber = new VehiclePartNumber(partNumber.Id, vehicle.Id, _fixture.Create<int>());
            _context.PartNumbers.Add(partNumber);
            _context.Suppliers.Add(supplier);
            _context.Vehicles.Add(vehicle);
            _context.PartNumberSuppliers.Add(partNumberSupplier);
            _context.VehiclePartNumbers.Add(vehiclePartNumber);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFullByIdAsync(partNumber.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Suppliers);
            var partNumberSupplierResult = result.Suppliers.First();
            Assert.Equal(partNumberSupplier.SupplierId, partNumberSupplierResult.SupplierId);
            Assert.Equal(partNumberSupplier.UnitPrice, partNumberSupplierResult.UnitPrice);
            Assert.NotNull(partNumberSupplier.Supplier);
            Assert.Equal(supplier, partNumberSupplier.Supplier);
            Assert.NotNull(partNumberSupplier.PartNumber);
            Assert.Equal(partNumber, partNumberSupplier.PartNumber);
            var vehiclePartNumberResult = result.Vehicles.First();
            Assert.Equal(vehiclePartNumber.VehicleId, vehiclePartNumberResult.VehicleId);
            Assert.Equal(vehiclePartNumber.Amount, vehiclePartNumberResult.Amount);
            Assert.NotNull(vehiclePartNumber.Vehicle);
            Assert.Equal(vehicle, vehiclePartNumber.Vehicle);
            Assert.NotNull(vehiclePartNumber.PartNumber);
            Assert.Equal(partNumber, vehiclePartNumber.PartNumber);
        }
    }
}
