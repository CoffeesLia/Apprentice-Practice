using Stellantis.ProjectName.Application.Interfaces.Repositories;

namespace Stellantis.ProjectName.Application.Interfaces
{
    public interface IUnitOfWork
    {
        public IAreaRepository AreaRepository { get; }

        public IIntegrationRepository IntegrationRepository { get; }

        public IResponsibleRepository ResponsibleRepository { get; }

        public IMemberRepository MemberRepository { get; }

        public IDataServiceRepository DataServiceRepository { get; }

        public IApplicationDataRepository ApplicationDataRepository { get; }                
        public ISquadRepository SquadRepository { get; }    

        public IGitRepoRepository GitRepoRepository { get; }

        Task CommitAsync();
        void BeginTransaction();
    }
}

