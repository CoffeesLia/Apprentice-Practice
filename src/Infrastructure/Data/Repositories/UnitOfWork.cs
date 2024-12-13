

using Domain.Interfaces;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data
{
    public class UnitOfWork(CleanArchBaseContext context, IPartNumberRepository partNumberRepository, IVehicleRepository vehicleRepository, IPartNumberVehicleRepository partNumberVehicleRepository, ISupplierRepository supplierRepository, IPartNumberSupplierRepository partNumberSupplierRepository) : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;

        public IPartNumberRepository PartNumberRepository
        {
            get
            {
                return partNumberRepository ??= new PartNumberRepository(context);
            }
        }

        public IVehicleRepository VehicleRepository
        {
            get
            {
                return vehicleRepository ??= new VehicleRepository(context);
            }
        }

        public IPartNumberVehicleRepository PartNumberVehicleRepository
        {
            get
            {
                return partNumberVehicleRepository ??= new PartNumberVehicleRepository(context);
            }
        }

        public ISupplierRepository SupplierRepository
        {
            get
            {
                return supplierRepository ??= new SupplierRepository(context);
            }
        }

        public IPartNumberSupplierRepository PartNumberSupplierRepository
        {
            get
            {
                return partNumberSupplierRepository ??= new PartNumberSupplierRepository(context);
            }
        }

        public void BeginTransaction()
        {
            _transaction = context.Database.BeginTransaction();
        }

        public Task Commit()
        {
            try
            {
                _transaction?.Commit();
            }
            catch
            {
                Rollback();
                throw;
            }

            return Task.CompletedTask;
        }

        private void Rollback()
        {
            _transaction?.Rollback();
        }

        public void DisposeIt()
        {
            _transaction?.Dispose();
        }


    }
}