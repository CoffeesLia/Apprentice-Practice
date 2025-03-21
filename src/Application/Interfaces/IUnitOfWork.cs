using Stellantis.ProjectName.Application.Interfaces.Repositories;

namespace Stellantis.ProjectName.Application.Interfaces
{
    public interface IUnitOfWork
    {
        public IAreaRepository AreaRepository { get; }

        IResponsibleRepository ResponsibleRepository { get; }

        public IApplicationDataRepository ApplicationDataRepository { get; }      
        
        public IDataServiceRepository DataServiceRepository { get; }

        Task CommitAsync();
        void BeginTransaction();
    }
}
