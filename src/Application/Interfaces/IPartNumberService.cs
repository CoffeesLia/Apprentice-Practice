using Domain.DTO;
using Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
