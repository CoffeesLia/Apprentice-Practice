using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Stellantis.ProjectName.Infrastructure.Data
{
    public class UnitOfWork(Context context, IStringLocalizer<DataServiceRepository> localizer) : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;

        public IAreaRepository AreaRepository { get; } = new AreaRepository(context);
        public IIntegrationRepository IntegrationRepository { get; } = new IntegrationRepository(context, localizer);
        public IResponsibleRepository ResponsibleRepository { get; } = new ResponsibleRepository(context);
        public IApplicationDataRepository ApplicationDataRepository { get; } = new ApplicationDataRepository(context);

        public void BeginTransaction()
        {
            _transaction = context.Database.BeginTransaction();
        }

        public async Task CommitAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync().ConfigureAwait(false);
                }
            }
            catch
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync().ConfigureAwait(false);
                }
                throw;
            }
        }
    }
}