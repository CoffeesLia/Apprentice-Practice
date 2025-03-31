using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Stellantis.ProjectName.Infrastructure.Repositories;

namespace Stellantis.ProjectName.Infrastructure.Data
{
    public class UnitOfWork(Context context) : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;
        private IDataServiceRepository? _dataServiceRepository;
        private ISquadRepository? _squadRepository;
        public IAreaRepository AreaRepository => throw new NotImplementedException();

        public IResponsibleRepository ResponsibleRepository => throw new NotImplementedException();

        public IApplicationDataRepository ApplicationDataRepository => throw new NotImplementedException();

        public IDataServiceRepository DataServiceRepository
        {
            get
            {
                return _dataServiceRepository ??= new DataServiceRepository(context);
            }
        }

        public ISquadRepository SquadRepository
        {
            get
            {
                return _squadRepository ??= new SquadRepository(context);
            }
        }

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
    }
}
