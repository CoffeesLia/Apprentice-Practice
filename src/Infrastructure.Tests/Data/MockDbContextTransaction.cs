using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Tests.Data
{
    [SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "Class only for tests.")]
    internal class MockDbContextTransaction : IDbContextTransaction
    {
        private bool _rollbackAsyncHasCalled;
        public Guid TransactionId => Guid.NewGuid();
        public void Commit()
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            _rollbackAsyncHasCalled = true;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            _rollbackAsyncHasCalled = true;
            return Task.CompletedTask;
        }
        internal bool RollbackAsyncHasCalled()
        {
            return _rollbackAsyncHasCalled;
        }
    }
}
