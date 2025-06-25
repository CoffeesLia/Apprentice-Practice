using Microsoft.EntityFrameworkCore.Storage;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Stellantis.ProjectName.Infrastructure.Data
{
    public class UnitOfWork(Context context) : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;
        private readonly Context _context = context;

        public IDocumentRepository DocumentDataRepository { get; } = new DocumentDataRepository(context);
        public IAreaRepository AreaRepository { get; } = new AreaRepository(context);
        public IIntegrationRepository IntegrationRepository { get; } = new IntegrationRepository(context);
        public IResponsibleRepository ResponsibleRepository { get; } = new ResponsibleRepository(context);
        public IFeedbackRepository FeedbackRepository { get; } = new FeedbackRepository(context);
        public IIncidentRepository IncidentRepository { get; } = new IncidentRepository(context);
        public IMemberRepository MemberRepository { get; } = new MemberRepository(context);
        public IApplicationDataRepository ApplicationDataRepository { get; } = new ApplicationDataRepository(context);
        public ISquadRepository SquadRepository { get; } = new SquadRepository(context);
        public IServiceDataRepository ServiceDataRepository { get; } = new ServiceDataRepository(context);
        public IManagerRepository ManagerRepository { get; } = new ManagerRepository(context);
        public IRepoRepository RepoRepository { get; } = new RepoRepository(context);

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
    }
}
