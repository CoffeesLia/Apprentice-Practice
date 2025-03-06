using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IApplicationDataService : IEntityServiceBase<ApplicationData>
    {
        Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter);
        Task<bool> IsAreaNameUniqueAsync(string nameApplication, int? id = null);

    }
}
