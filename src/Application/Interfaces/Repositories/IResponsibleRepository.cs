using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;


namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IResponsibleRepository : IRepositoryEntityBase<Responsible>
    {
        Task<Responsible>GetByEmailAsync(string email);
        Task<bool> VerifyEmailAlreadyExistsAsync(string email);
        Task<PagedResult<Responsible>> GetListAsync(ResponsibleFilter filter);

    }
}
