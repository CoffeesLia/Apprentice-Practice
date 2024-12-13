using Domain.DTO;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IPartNumberRepository : IBaseRepository<PartNumber>
    {
        bool VerifyCodeExists(string code);
        Task<PaginationDTO<PartNumber>> GetListFilter(PartNumberFilterDTO filter);

    }
}
