using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Domain.Entities;


namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IDocumentService : IEntityServiceBase<DocumentData>
    {
      //  Task<bool> IsDocumentNameUniqueAsync(string name, int? id = null);

    }
}
