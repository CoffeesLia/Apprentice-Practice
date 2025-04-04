using Microsoft.EntityFrameworkCore.Storage;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Stellantis.ProjectName.Infrastructure.Repositories;

namespace Stellantis.ProjectName.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;
        private readonly Context _context;

        public IAreaRepository AreaRepository { get; }
        public IIntegrationRepository IntegrationRepository { get; }
        public IResponsibleRepository ResponsibleRepository { get; }
        public IMemberRepository MemberRepository { get; }
        public IApplicationDataRepository ApplicationDataRepository { get; }
        public ISquadRepository SquadRepository { get; }
        public IDataServiceRepository DataServiceRepository { get; }
        public IGitRepoRepository GitRepoRepository { get; }
        
        public UnitOfWork(Context context)
        {
            _context = context;
            AreaRepository = new AreaRepository(context);
            IntegrationRepository = new IntegrationRepository(context);
            ResponsibleRepository = new ResponsibleRepository(context);
            MemberRepository = new MemberRepository(context);
            ApplicationDataRepository = new ApplicationDataRepository(context);
            SquadRepository = new SquadRepository(context);
        }

        public void BeginTransaction()
        {
            _transaction = _context.Database.BeginTransaction();
        }

        public async Task CommitAsync()
        {
            try
            {
                if (_transaction != null)
                    await _transaction.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                if (_transaction != null)
                    await _transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
    }
}