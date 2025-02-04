using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Infrastructure.Tests.Data.Repositories
{
    public class UnitOfWorkTests
    {
        private readonly Context _context;
        private readonly Mock<IPartNumberRepository> _mockPartNumberRepository = new();
        private readonly Mock<IVehicleRepository> _mockVehicleRepository = new();
        private readonly Mock<ISupplierRepository> _mockSupplierRepository = new();
        private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
        private readonly UnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: nameof(UnitOfWorkTests))
                .Options;
            _context = new Context(options);
            _unitOfWork = new UnitOfWork(
                _context,
                _mockPartNumberRepository.Object,
                _mockVehicleRepository.Object,
                _mockSupplierRepository.Object,
                _mockEmployeeRepository.Object
            );
        }

        [Fact]
        public void Create_WhenNullForRepositories()
        {
            // Act
            UnitOfWork unitOfWork = new(_context);

            // Assert
            Assert.NotNull(unitOfWork.EmployeeRepository);
            Assert.NotNull(unitOfWork.PartNumberRepository);
            Assert.NotNull(unitOfWork.SupplierRepository);
            Assert.NotNull(unitOfWork.VehicleRepository);
        }


        [Fact]
        public void PartNumberRepository_ReturnRepositoryInstance()
        {
            // Act
            var repository = _unitOfWork.PartNumberRepository;

            // Assert
            Assert.NotNull(repository);
            Assert.Equal(_mockPartNumberRepository.Object, repository);
        }

        [Fact]
        public void VehicleRepository_ReturnRepositoryInstance()
        {
            // Act
            var repository = _unitOfWork.VehicleRepository;

            // Assert
            Assert.NotNull(repository);
            Assert.Equal(_mockVehicleRepository.Object, repository);
        }

        [Fact]
        public void SupplierRepository_ReturnRepositoryInstance()
        {
            // Act
            var repository = _unitOfWork.SupplierRepository;

            // Assert
            Assert.NotNull(repository);
            Assert.Equal(_mockSupplierRepository.Object, repository);
        }

        [Fact]
        public void BeginTransaction_ShouldStartTransaction()
        {
            // Arrange
            var mockTransaction = new Mock<IDbContextTransaction>();
            var mockDatabase = new Mock<DatabaseFacade>(_context);
            mockDatabase.Setup(db => db.BeginTransaction()).Returns(mockTransaction.Object);
            var context = new Mock<Context>(new DbContextOptions<Context>());
            context.Setup(c => c.Database).Returns(mockDatabase.Object);

            var unitOfWork = new UnitOfWork(
                context.Object,
                _mockPartNumberRepository.Object,
                _mockVehicleRepository.Object,
                _mockSupplierRepository.Object);

            // Act
            unitOfWork.BeginTransaction();

            // Assert
            mockDatabase.Verify(db => db.BeginTransaction(), Times.Once);
        }

        [Fact]
        public async Task CommitAsync_WhenNotBebunTransction()
        {
            await _unitOfWork.CommitAsync();
        }

        [Fact]
        public void DisposeIt_WhenTransactionIsNull()
        {
            _unitOfWork.DisposeIt();
        }

        [Fact]
        public void DisposeIt()
        {
            // Arrange
            var mockTransaction = new Mock<IDbContextTransaction>();
            var mockDatabase = new Mock<DatabaseFacade>(_context);
            mockDatabase.Setup(db => db.BeginTransaction()).Returns(mockTransaction.Object);
            var context = new Mock<Context>(new DbContextOptions<Context>());
            context.Setup(c => c.Database).Returns(mockDatabase.Object);
            var unitOfWork = new UnitOfWork(context.Object);

            // Act
            unitOfWork.BeginTransaction();
            unitOfWork.DisposeIt();

            // Assert
            mockTransaction.Verify(t => t.Dispose(), Times.Once);
        }

        [Fact]
        public async Task CommitAsync_ShouldCallSaveChangesOnContext()
        {
            // Arrange
            var mockTransaction = new Mock<IDbContextTransaction>();
            var mockDatabase = new Mock<DatabaseFacade>(_context);
            mockDatabase.Setup(db => db.BeginTransaction()).Returns(mockTransaction.Object);
            var context = new Mock<Context>(new DbContextOptions<Context>());
            context.Setup(c => c.Database).Returns(mockDatabase.Object);
            context.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            var unitOfWork = new UnitOfWork(context.Object);

            unitOfWork.BeginTransaction();

            // Act
            await unitOfWork.CommitAsync();

            // Assert
            mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CommitAsync_ShouldRollbackTransactionOnException()
        {
            // Arrange
            var mockTransaction = new MockDbContextTransaction();
            var mockDatabase = new Mock<DatabaseFacade>(_context);
            mockDatabase
                .Setup(db => db.BeginTransaction())
                .Returns(mockTransaction);
            var context = new Mock<Context>(new DbContextOptions<Context>());
            context
                .Setup(c => c.Database)
                .Returns(mockDatabase.Object);
            var unitOfWork = new UnitOfWork(context.Object);

            unitOfWork.BeginTransaction();

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => unitOfWork.CommitAsync());
            Assert.True(mockTransaction.RollbackAsyncHasCalled());
        }
    }
}
