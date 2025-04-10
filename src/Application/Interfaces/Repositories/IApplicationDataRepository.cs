using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IApplicationDataRepository : IRepositoryEntityBase<ApplicationData>
    {
        //teste
        Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter);
        Task<bool> IsApplicationNameUniqueAsync(string name, int? id = null);
        Task<ApplicationData?> GetFullByIdAsync(int id);
        Task<bool> IsResponsibleFromArea(int areaId, int responsibleId);

    }
}
