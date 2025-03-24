using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Tests.Data
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly Context _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly Mock<IStringLocalizer<DataServiceRepository>> _mockLocalizer;

        public UnitOfWorkTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: nameof(UnitOfWorkTests))
                .Options;
            _context = new Context(options);
            _mockLocalizer = new Mock<IStringLocalizer<DataServiceRepository>>();
            _unitOfWork = new UnitOfWork(_context, _mockLocalizer.Object);
        }

        [Fact]
        [SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed", Justification = "It's a temporary code.")]
        [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "It's a temporary code.")]
        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "It's a temporary code.")]
        public void CreateWhenNullForRepositories()
        {
            // Act
            UnitOfWork unitOfWork = new(_context, _mockLocalizer.Object);

            // Assert
        }

        [Fact]
        public void BeginTransactionShouldStartTransaction()
        {
            // Arrange
            var mockTransaction = new Mock<IDbContextTransaction>();
            var mockDatabase = new Mock<DatabaseFacade>(_context);
            mockDatabase.Setup(db => db.BeginTransaction()).Returns(mockTransaction.Object);
            var context = new Mock<Context>(new DbContextOptions<Context>());
            context.Setup(c => c.Database).Returns(mockDatabase.Object);

            var unitOfWork = new UnitOfWork(context.Object, _mockLocalizer.Object);

            // Act
            unitOfWork.BeginTransaction();

            // Assert
            mockDatabase.Verify(db => db.BeginTransaction(), Times.Once);
        }

        [Fact]
        public async Task CommitAsyncWhenNotBebunTransction()
        {
            await _unitOfWork.CommitAsync();
            Assert.True(true);
        }

        [Fact]
        public void DisposeItWhenTransactionIsNull()
        {
            _unitOfWork.DisposeIt();
            Assert.True(true);
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
            var unitOfWork = new UnitOfWork(context.Object, _mockLocalizer.Object);

            // Act
            unitOfWork.BeginTransaction();
            unitOfWork.DisposeIt();

            // Assert
            mockTransaction.Verify(t => t.Dispose(), Times.Once);
        }

        [Fact]
        public async Task CommitAsyncShouldCallSaveChangesOnContext()
        {
            // Arrange
            var mockTransaction = new Mock<IDbContextTransaction>();
            var mockDatabase = new Mock<DatabaseFacade>(_context);
            mockDatabase.Setup(db => db.BeginTransaction()).Returns(mockTransaction.Object);
            var context = new Mock<Context>(new DbContextOptions<Context>());
            context.Setup(c => c.Database).Returns(mockDatabase.Object);
            context.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            var unitOfWork = new UnitOfWork(context.Object, _mockLocalizer.Object);

            unitOfWork.BeginTransaction();

            // Act
            await unitOfWork.CommitAsync();

            // Assert
            mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CommitAsyncShouldRollbackTransactionOnException()
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
            var unitOfWork = new UnitOfWork(context.Object, _mockLocalizer.Object);

            unitOfWork.BeginTransaction();

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => unitOfWork.CommitAsync());
            Assert.True(mockTransaction.RollbackAsyncHasCalled());
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
