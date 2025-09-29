using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IDocumentService : IEntityServiceBase<DocumentData>
    {
        Task<PagedResult<DocumentData>> GetListAsync(DocumentDataFilter filter);

    }
}
