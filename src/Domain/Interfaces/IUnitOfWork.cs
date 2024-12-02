namespace Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IPartNumberRepository PartNumberRepository { get; }
        IVehicleRepository VehicleRepository { get; }
        IPartNumberVehicleRepository PartNumberVehicleRepository { get; }
        ISupplierRepository SupplierRepository { get; }
        IPartNumberSupplierRepository PartNumberSupplierRepository { get; }
        Task Commit();
        void BeginTransaction();
    }
}
