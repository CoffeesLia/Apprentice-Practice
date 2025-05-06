using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class EmployeeRepositoryTests
    {
        private readonly IFixture _fixture;
        private readonly Context _context;
        private readonly EmployeeRepository _repository;

        public EmployeeRepositoryTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new EmployeeRepository(_context);
        }

        [Fact]
        public async Task CreateAsync_Success()
        {
            // Arrange
            var employee = _fixture.Create<Employee>();

            // Act
            var repository = new EmployeeRepository(_context);
            await repository.CreateAsync(employee);
            var result = await _context.Employees.FindAsync([employee.Id]);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(employee.Name, result.Name);
        }

        [Fact]
        public async Task DeleteAsync_Success()
        {
            // Arrange
            var employee = _fixture.Create<Employee>();
            _context.Add(employee);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(employee.Id);
            var result = await _repository.GetByIdAsync(employee.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_Success()
        {
            // Arrange
            var employee = _fixture.Create<Employee>();
            _context.Add(employee);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(employee.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(employee.Name, result.Name);
        }

        [Fact]
        public async Task GetListAsync_Success()
        {
            // Arrange
            var employees = _fixture.CreateMany<Employee>(5).ToList();
            await _repository.CreateAsync(employees);

            // Act
            var result = await _repository.GetListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Result.Count());
        }

        [Fact]
        public async Task UpdateAsync_Success()
        {
            // Arrange
            var employee = _fixture.Create<Employee>();
            await _repository.CreateAsync(employee);
            employee.Name = "Updated Name";

            // Act
            await _repository.UpdateAsync(employee);
            var result = await _repository.GetByIdAsync(employee.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(employee.Name, result.Name);
        }
    }
}



