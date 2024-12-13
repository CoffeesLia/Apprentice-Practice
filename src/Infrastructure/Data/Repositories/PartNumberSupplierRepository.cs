using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data.Context;

namespace Infrastructure.Data.Repositories
{
    public class PartNumberSupplierRepository(CleanArchBaseContext context) : BaseRepository<PartNumberSupplier>(context), IPartNumberSupplierRepository
    {
    }
}
