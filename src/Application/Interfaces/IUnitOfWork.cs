using Stellantis.ProjectName.Application.Interfaces.Repositories;

namespace Stellantis.ProjectName.Application.Interfaces
{
    public interface IUnitOfWork
    {
        public IAreaRepository AreaRepository { get; }

        public IIntegrationRepository IntegrationRepository { get; }
        IResponsibleRepository ResponsibleRepository { get; }
        public IDataServiceRepository DataServiceRepository { get; }
        public IApplicationDataRepository ApplicationDataRepository { get; }                

        Task CommitAsync();
        void BeginTransaction();
    }
}

