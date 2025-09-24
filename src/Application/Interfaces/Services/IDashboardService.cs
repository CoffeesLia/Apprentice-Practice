using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<Dashboard> GetDashboardAsync();
    }
}