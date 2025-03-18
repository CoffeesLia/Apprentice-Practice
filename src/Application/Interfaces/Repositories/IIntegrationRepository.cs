using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IIntegrationRepository : IRepositoryEntityBase<Integration>
    {
        Task<IEnumerable<Integration>> GetAllAsync();
        new Task<PagedResult<Integration>> CreateAsync(Integration integration);
        Task<PagedResult<Integration>> UpdateAsync(Integration integration);
        Task DeleteAsync(int id);
        Task<Integration?> GetByIdAsync(int id);
    }
}
