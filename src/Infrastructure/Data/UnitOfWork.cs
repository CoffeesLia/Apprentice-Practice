using Microsoft.EntityFrameworkCore.Storage;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Stellantis.ProjectName.Infrastructure.Data
{
    public class UnitOfWork(Context context, IPartNumberRepository? partNumberRepository = null, IVehicleRepository? vehicleRepository = null, ISupplierRepository? supplierRepository = null, IEmployeeRepository? employeeRepository = null) : IUnitOfWork
    {
        public IEmployeeRepository EmployeeRepository => employeeRepository ??= new EmployeeRepository(context);
        public IPartNumberRepository PartNumberRepository => partNumberRepository ??= new PartNumberRepository(context);
        public IVehicleRepository VehicleRepository => vehicleRepository ??= new VehicleRepository(context);
        public ISupplierRepository SupplierRepository => supplierRepository ??= new SupplierRepository(context);
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
