using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Infrastructure.Tests.Data
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly Context _context;
        private readonly UnitOfWork _unitOfWork;
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

        public UnitOfWorkTests()
        {
            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: nameof(UnitOfWorkTests))
                .Options;
            _context = new Context(options);
            _unitOfWork = new UnitOfWork(_context);
        }

        [Fact]
        public void BeginTransactionShouldStartTransaction()
        {
            // Arrange
            Mock<IDbContextTransaction> mockTransaction = new();
            Mock<DatabaseFacade> mockDatabase = new(_context);
            mockDatabase.Setup(db => db.BeginTransaction()).Returns(mockTransaction.Object);
            Mock<Context> context = new(new DbContextOptions<Context>());
            context.Setup(c => c.Database).Returns(mockDatabase.Object);

            UnitOfWork unitOfWork = new(context.Object);

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
        public async Task CommitAsyncShouldCallSaveChangesOnContext()
        {
            // Arrange
            Mock<IDbContextTransaction> mockTransaction = new();
            Mock<DatabaseFacade> mockDatabase = new(_context);
            mockDatabase.Setup(db => db.BeginTransaction()).Returns(mockTransaction.Object);
            Mock<Context> context = new(new DbContextOptions<Context>());
            context.Setup(c => c.Database).Returns(mockDatabase.Object);
            context.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            UnitOfWork unitOfWork = new(context.Object);

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
            using MockDbContextTransaction mockTransaction = new();
            Mock<DatabaseFacade> mockDatabase = new(_context);
            mockDatabase
                .Setup(db => db.BeginTransaction())
                .Returns(mockTransaction);
            Mock<Context> context = new(new DbContextOptions<Context>());
            context
                .Setup(c => c.Database)
                .Returns(mockDatabase.Object);
            UnitOfWork unitOfWork = new(context.Object);

            unitOfWork.BeginTransaction();

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => unitOfWork.CommitAsync());
            Assert.True(mockTransaction.RollbackAsyncHasCalled());
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // free managed resources
                _context?.Dispose();
            }

            // free native resources if there are any.
            if (nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeResource);
                nativeResource = IntPtr.Zero;
            }

            isDisposed = true;
        }
    }
}

