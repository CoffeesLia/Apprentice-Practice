using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IAreaService : IEntityServiceBase<Area>
    {
        Task<PagedResult<Area>> GetListAsync(AreaFilter areaFilter);
    }
}
