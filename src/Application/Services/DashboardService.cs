using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Dashboard> GetDashboardAsync()
        {
            var applications = await _unitOfWork.ApplicationDataRepository
                .GetListAsync(a => true).ConfigureAwait(false);
            int totalApplications = applications.Count;

            var incidents = await _unitOfWork.IncidentRepository
                .GetListAsync(i => i.Status == IncidentStatus.Open).ConfigureAwait(false);
            int totalOpenIncidents = incidents.Total;

            var members = await _unitOfWork.MemberRepository
                .GetListAsync(m => true).ConfigureAwait(false);
            int totalMembers = members.Total;

            var squads = await _unitOfWork.SquadRepository
                .GetListAsync(s => s.Members != null && s.Members.Any())
                .ConfigureAwait(false);

            var squadSummaries = squads.Result.Select(s => new SquadSummary
            {
                SquadName = s.Name!,
                Members = s.Members?.Select(m => new MemberSummary
                {
                    Name = m.Name,
                    Role = m.Role
                }).ToList() ?? []
            }).ToList();

            return new Dashboard
            {
                TotalApplications = totalApplications,
                TotalOpenIncidents = totalOpenIncidents,
                TotalMembers = totalMembers,
                Squads = squadSummaries
            };
        }
    }
}