using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Dashboard> GetDashboardDataAsync()
        {
            // Total de aplicações
            var applications = await _unitOfWork.ApplicationDataRepository
                .GetListAsync(a => true).ConfigureAwait(false);
            int totalApplications = applications.Count;

            // Total de incidentes abertos
            var incidents = await _unitOfWork.IncidentRepository
                .GetListAsync(i => i.Status == IncidentStatus.Open).ConfigureAwait(false);
            int openIncidents = incidents.Result.Count();

            // Total de squads ativos
            var squads = await _unitOfWork.SquadRepository
                .GetListAsync(s => s.Members != null && s.Members.Any()).ConfigureAwait(false);
            int activeSquads = squads.Result.Count();

            // Retorna os dados agregados
            return new Dashboard(totalApplications, openIncidents, activeSquads);
        }
    }
}