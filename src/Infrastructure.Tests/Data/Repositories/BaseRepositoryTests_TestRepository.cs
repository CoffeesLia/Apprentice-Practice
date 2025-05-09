using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public partial class RepositoryBaseTests
    {
        internal sealed class TestRepository(TestContext context) : RepositoryBase<TestEntity, TestContext>(context)
        {
            /// <summary>
            /// To test the protected method GetListAsync
            /// </summary>
            internal async Task<PagedResult<TestEntity>> CallGetListAsync(IQueryable<TestEntity> query, string? sort = null, string? sortDir = null, int page = 1, int pageSize = 10)
            {
                return await GetListAsync(query, sort, sortDir, page, pageSize).ConfigureAwait(false);
            }
        }
    }
}
