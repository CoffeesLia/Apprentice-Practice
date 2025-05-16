using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IDocumentRepository : IRepositoryEntityBase<DocumentData>
    {
        Task<bool> IsDocumentNameUniqueAsync(string name, int? id = null);
        Task<bool> IsUrlUniqueAsync(Uri url, int? id = null);

    }
}
