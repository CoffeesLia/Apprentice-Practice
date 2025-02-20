using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    public interface IAreaControllerBase
    {
        Task<IActionResult> CreateAsync([FromBody] AreaDto itemDto);
        Task<IActionResult> DeleteListAsync(int id);
        Task<IActionResult> EditarAreaAsync(int id, [FromBody] AreaDto areaAtualizadaDto);
        Task<IActionResult> GetAsync(int id);
        Task<IActionResult> GetListAsync([FromQuery] AreaFilterDto filterDto);
    }
}