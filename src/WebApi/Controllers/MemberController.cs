
using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.ViewModels;
using Microsoft.Extensions.Localization;
using AutoMapper;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.Application.Models.Filters;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/members")]
  
    internal sealed class MemberControllerBase(IMemberService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Member, MemberDto>(service, mapper, localizerFactory)
    {
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] MemberDto itemDto)
        {
            return await CreateBaseAsync<MemberVm>(itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MemberVm>> GetAsync(int id)
        {
            return await GetAsync<MemberVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] MemberFilterDto filterDto)
        {
            var filter = Mapper.Map<MemberFilter>(filterDto);
            var pagedResult = await ((IMemberService)Service).GetListAsync(filter!).ConfigureAwait(false);
            var result = Mapper.Map<PagedResultVm<MemberVm>>(pagedResult);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] MemberDto itemDto)
        {
            return await base.UpdateBaseAsync<MemberVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

    }

}
