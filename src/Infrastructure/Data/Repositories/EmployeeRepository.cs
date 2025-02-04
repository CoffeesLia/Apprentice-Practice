using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class EmployeeRepository(Context context) : BaseRepositoryEntity<Employee, Context>(context), IEmployeeRepository
    {
    }
}
