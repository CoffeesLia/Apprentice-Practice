using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Stellantis.ProjectName.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;
        private readonly Context context;

        public UnitOfWork(Context context, IStringLocalizer<DataServiceRepository> localizer)
        {
            this.context = context;
            AreaRepository = new AreaRepository(context);
            IntegrationRepository = new IntegrationRepository(context, localizer);
            ResponsibleRepository = new ResponsibleRepository(context);
            ApplicationDataRepository = new ApplicationDataRepository(context);
            DataServiceRepository = new DataServiceRepository(context, localizer);
        }

        public IAreaRepository AreaRepository { get; }
        public IIntegrationRepository IntegrationRepository { get; }
        public IResponsibleRepository ResponsibleRepository { get; }
        public IApplicationDataRepository ApplicationDataRepository { get; }
        public IDataServiceRepository DataServiceRepository { get; }

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

        internal void DisposeIt()
        {
            throw new NotImplementedException();
        }
    }
}