using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;


namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IDocumentService : IEntityServiceBase<DocumentData>
    {
        Task<PagedResult<DocumentData>> GetListAsync(DocumentDataFilter filter);

    }
}
