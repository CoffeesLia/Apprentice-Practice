using Stellantis.ProjectName.Application.Interfaces.Repositories;

namespace Stellantis.ProjectName.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IAreaRepository AreaRepository { get; }
        Task CommitAsync();
        void BeginTransaction();
    }
}
