using Stellantis.ProjectName.Application.Interfaces.Repositories;

namespace Stellantis.ProjectName.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task CommitAsync();
        void BeginTransaction();
    }
}
