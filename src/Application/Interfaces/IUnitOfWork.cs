using Stellantis.ProjectName.Application.Interfaces.Repositories;

namespace Stellantis.ProjectName.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IAreaRepository AreaRepository { get; }
        IIntegrationRepository IntegrationRepository { get; }
        IResponsibleRepository ResponsibleRepository { get; }
        IApplicationDataRepository ApplicationDataRepository { get; }
        IDataServiceRepository DataServiceRepository { get; }
        ISquadRepository SquadRepository { get; }
        IGitRepoRepository GitRepoRepository { get; } 
        Task CommitAsync();
        void BeginTransaction();
    }
}
