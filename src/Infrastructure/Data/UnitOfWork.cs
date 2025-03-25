using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Stellantis.ProjectName.Infrastructure.Repositories;
namespace Stellantis.ProjectName.Infrastructure.Data

{

<<<<<<< HEAD
    public class UnitOfWork : IUnitOfWork
=======
public class UnitOfWork(Context context) : IUnitOfWork

    public class UnitOfWork(Context context) : IUnitOfWork

>>>>>>> 73544512e72b6c7457482095c02941d5fde775f8
    {
        private IDbContextTransaction? _transaction;
        private readonly Context _context;

<<<<<<< HEAD
        public IAreaRepository AreaRepository { get; }
        public IIntegrationRepository IntegrationRepository { get; }
        public IResponsibleRepository ResponsibleRepository { get; }
        public IApplicationDataRepository ApplicationDataRepository { get; }
        public ISquadRepository SquadRepository { get; }
        public IDataServiceRepository DataServiceRepository { get; }

        // Construtor corrigido
        public UnitOfWork(Context context)
        {
            _context = context;
            AreaRepository = new AreaRepository(context);
            IntegrationRepository = new IntegrationRepository(context);
            ResponsibleRepository = new ResponsibleRepository(context);
            ApplicationDataRepository = new ApplicationDataRepository(context);
            SquadRepository = new SquadRepository(context);
        }
=======

        public IAreaRepository AreaRepository { get; } = new AreaRepository(context);
        public IIntegrationRepository IntegrationRepository { get; } = new IntegrationRepository(context);
        public IResponsibleRepository ResponsibleRepository { get; } = new ResponsibleRepository(context);
        public IApplicationDataRepository ApplicationDataRepository { get; } = new ApplicationDataRepository(context);
        public ISquadRepository SquadRepository => throw new NotImplementedException();
        public IDataServiceRepository DataServiceRepository => throw new NotImplementedException();
>>>>>>> 73544512e72b6c7457482095c02941d5fde775f8

        
        public void BeginTransaction()
        {
            _transaction = _context.Database.BeginTransaction();
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

