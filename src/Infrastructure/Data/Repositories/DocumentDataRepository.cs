using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class DocumentDataRepository(Context context) : RepositoryBase<DocumentData, Context>(context), IDocumentRepository
    {
        public async Task DeleteAsync(int id, bool saveChanges = true)
        {

           DocumentData? entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<DocumentData>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<DocumentData?> GetByIdAsync(int id)
        {
            return await Context.Set<DocumentData>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<bool> IsDocumentNameUniqueAsync(string name, int? id = null)
        {
           return await Context.Set<DocumentData>().AnyAsync(a => a.Name == name && (!id.HasValue || a.Id != id)).ConfigureAwait(false);
        }


        public async Task<bool> IsUrlUniqueAsync(Uri url, int? id = null)
         {
             return await Context.Set<DocumentData>().AnyAsync(a => a.Url == url && (!id.HasValue || a.Id !=id)).ConfigureAwait(false);
         }
    }
}
