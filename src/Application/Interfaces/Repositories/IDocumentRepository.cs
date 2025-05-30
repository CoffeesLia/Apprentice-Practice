using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IDocumentRepository : IRepositoryEntityBase<DocumentData>
    {
        Task<PagedResult<DocumentData>> GetListAsync(DocumentDataFilter documentFilter);
        Task<bool> IsDocumentNameUniqueAsync(string name, int applicationId, int? id = null);
        Task<bool> IsUrlUniqueAsync(Uri url, int applicationId, int? id = null);

    }
}
