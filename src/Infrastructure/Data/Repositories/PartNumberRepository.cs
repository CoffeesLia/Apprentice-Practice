using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;

using Infrastructure.Data.Context;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class PartNumberRepository(CleanArchBaseContext context) : BaseRepository<PartNumber>(context), IPartNumberRepository
    {
        public bool VerifyCodeExists(string code)
        {
            return _context.PartNumber.Any(p => p.Code == code);
        }

        public async Task<PaginationDTO<PartNumber>> GetListFilter(PartNumberFilterDTO filter)
        {
            var filters = PredicateBuilder.New<PartNumber>(true);

            if (!string.IsNullOrWhiteSpace(filter.Code))
                filters = filters.And(x => x.Code.Contains(filter.Code));
            if(!string.IsNullOrWhiteSpace(filter.Description))
                filters = filters.And(x => x.Description.Contains(filter.Description));
            if(!filter.Type.Equals(null))
                filters = filters.And(x => x.Type.Equals(filter.Type));


            return await GetListAsync(filter: filters, page: filter.Page, sort: filter.Sort, sortDir: filter.SortDir);
        }

    }
}
