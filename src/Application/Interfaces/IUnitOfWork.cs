using Stellantis.ProjectName.Application.Interfaces.Repositories;

namespace Stellantis.ProjectName.Application.Interfaces
{
    public interface IUnitOfWork
    {

        IDocumentRepository DocumentDataRepository { get; } 

        IAreaRepository AreaRepository { get; }

        IIntegrationRepository IntegrationRepository { get; }

        IResponsibleRepository ResponsibleRepository { get; }

        IIncidentRepository IncidentRepository { get; }

        IMemberRepository MemberRepository { get; }

        IServiceDataRepository ServiceDataRepository { get; }

        IApplicationDataRepository ApplicationDataRepository { get; }

        ISquadRepository SquadRepository { get; }

        IGitRepoRepository GitRepoRepository { get; }

        IManagerRepository ManagerRepository { get; }

        Task CommitAsync();
        void BeginTransaction();
    }
}

