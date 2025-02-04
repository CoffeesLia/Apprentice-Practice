using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class SupplierRepositoryTests
    {
        private readonly Context _context;
        private readonly SupplierRepository _repository;
        private Fixture _fixture;

        public SupplierRepositoryTests()
        {
            _fixture = new Fixture();
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
            .Options;
            _context = new Context(options);
            _repository = new SupplierRepository(_context);
        }

        [Fact]
        public async Task GetListAsync_ByAddress()
        {
            // Arrange
            var filter = new SupplierFilter
            {
                Page = 1,
                RowsPerPage = 10,
                Address = _fixture.Create<string>()
            };
            const int Count = 10;
            var suppliers = _fixture
                .Build<Supplier>()
                .With(x => x.Address, filter.Address)
                .CreateMany<Supplier>(Count);
            await _context.Suppliers.AddRangeAsync(suppliers);
            suppliers = _fixture
                .Build<Supplier>()
                .With(x => x.Address, _fixture.Create<string>())
                .CreateMany<Supplier>(Count);
            await _context.Suppliers.AddRangeAsync(suppliers);
            await _context.SaveChangesAsync();

            // Act
            var list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
        }

        [Fact]
        public async Task GetListAsync_ByCompanyName()
        {
            // Arrange
            var filter = new SupplierFilter
            {
                Page = 1,
                RowsPerPage = 10,
                CompanyName = _fixture.Create<string>()
            };
            const int Count = 10;
            var suppliers = _fixture
                .Build<Supplier>()
                .With(x => x.CompanyName, filter.CompanyName)
                .CreateMany<Supplier>(Count);
            await _context.Suppliers.AddRangeAsync(suppliers);
            suppliers = _fixture
                .Build<Supplier>()
                .With(x => x.CompanyName, _fixture.Create<string>())
                .CreateMany<Supplier>(Count);
            await _context.Suppliers.AddRangeAsync(suppliers);
            await _context.SaveChangesAsync();

            // Act
            var list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
        }

        [Fact]
        public async Task GetListAsync_ByCode()
        {
            // Arrange
            var filter = new SupplierFilter
            {
                Page = 1,
                RowsPerPage = 10,
                Code = _fixture.Create<string>()
            };
            const int Count = 10;
            var suppliers = _fixture
                .Build<Supplier>()
                .With(x => x.Code, filter.Code)
                .CreateMany<Supplier>(Count);
            await _context.Suppliers.AddRangeAsync(suppliers);
            suppliers = _fixture
                .Build<Supplier>()
                .With(x => x.Code, _fixture.Create<string>())
                .CreateMany<Supplier>(Count);
            await _context.Suppliers.AddRangeAsync(suppliers);
            await _context.SaveChangesAsync();

            // Act
            var list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
        }

        [Fact]
        public async Task GetListAsync_ByPhone()
        {
            // Arrange
            var filter = new SupplierFilter
            {
                Page = 1,
                RowsPerPage = 10,
                Phone = _fixture.Create<string>()
            };
            const int Count = 10;
            var suppliers = _fixture
                .Build<Supplier>()
                .With(x => x.Phone, filter.Phone)
                .CreateMany<Supplier>(Count);
            await _context.Suppliers.AddRangeAsync(suppliers);
            suppliers = _fixture
                .Build<Supplier>()
                .With(x => x.Phone, _fixture.Create<string>())
                .CreateMany<Supplier>(Count);
            await _context.Suppliers.AddRangeAsync(suppliers);
            await _context.SaveChangesAsync();

            // Act
            var list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
        }

        [Fact]
        public async Task GetFullByIdAsync_ReturnNull_WhenNotFound()
        {
            // Act
            var result = await _repository.GetFullByIdAsync(0);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetFullByIdAsync_Success()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            await _context.Suppliers.AddRangeAsync(supplier);
            var partnumbers = _fixture
                .Build<PartNumberSupplier>()
                .With(x => x.SupplierId, supplier.Id)
                .CreateMany(10);
            await _context.PartNumberSuppliers.AddRangeAsync(partnumbers);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFullByIdAsync(supplier.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(supplier.Address, result.Address);
            Assert.Equal(supplier.Code, result.Code);
            Assert.Equal(supplier.CompanyName, result.CompanyName);
            Assert.Equal(supplier.Phone, result.Phone);
            Assert.Equal(partnumbers, supplier.PartNumbers);
        }

        [Fact]
        public async Task VerifyCodeExistsAsync_ReturnTrue_WhenCodeExists()
        {
            // Arrange
            var suppliers = new List<Supplier>
            {
                new Supplier("Code1", "Company1", "Phone1", "Address1")
            }.AsQueryable();
            await _repository.CreateAsync(suppliers);

            // Act
            var result = await _repository.VerifyCodeExistsAsync("Code1");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyCodeExistsAsync_ReturnFalse_WhenCodeDoesNotExist()
        {
            // Arrange
            var suppliers = new List<Supplier>
            {
                new Supplier("Code1", "Company1", "Phone1", "Address1")
            }.AsQueryable();
            await _repository.CreateAsync(suppliers);

            // Act
            var result = await _repository.VerifyCodeExistsAsync("Code2");

            // Assert
            Assert.False(result);
        }
    }
}
