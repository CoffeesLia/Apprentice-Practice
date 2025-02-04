using Stellantis.ProjectName.Application.Interfaces.Repositories;

namespace Stellantis.ProjectName.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IPartNumberRepository PartNumberRepository { get; }
        IVehicleRepository VehicleRepository { get; }
        ISupplierRepository SupplierRepository { get; }
        IEmployeeRepository EmployeeRepository { get; }

        Task CommitAsync();
        void BeginTransaction();
    }
}
