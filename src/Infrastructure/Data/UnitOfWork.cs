using Microsoft.EntityFrameworkCore.Storage;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;

namespace Stellantis.ProjectName.Infrastructure.Data
{
    public class UnitOfWork(Context context) : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;

        public void BeginTransaction()
        {
            _transaction = context.Database.BeginTransaction();
        }

        public Task CommitAsync()
        {
            try
            {
                _transaction?.CommitAsync();
            }
            catch
            {
                _transaction!.RollbackAsync();
                throw;
            }
            return Task.CompletedTask;
        }

        public void DisposeIt()
        {
            _transaction?.Dispose();
        }
    }
}
