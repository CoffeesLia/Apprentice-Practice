using Domain.DTO;

namespace Application.Interfaces
{
    public interface IPartNumberService
    {
        Task Create(PartNumberDTO partNumberDTO);
        Task Delete(int id);
        Task<PartNumberDTO> Get(int id);
        Task<PaginationDTO<PartNumberDTO>> GetList(PartNumberFilterDTO filter);
        Task Update(PartNumberDTO partNumberDTO);
    }
}
