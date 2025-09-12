using Stellantis.ProjectName.Domain.Entities;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<Dashboard> GetDashboardAsync();
    }
}