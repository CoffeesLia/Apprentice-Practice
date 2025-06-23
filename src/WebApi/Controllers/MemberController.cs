
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/members")]

    public sealed class MemberController(IMemberService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Member, MemberDto>(service, mapper, localizerFactory)
    {

        protected override IMemberService Service => (IMemberService)base.Service;


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
            MemberFilter filter = Mapper.Map<MemberFilter>(filterDto);
            PagedResult<Member> result = await Service.GetListAsync(filter).ConfigureAwait(false);
            return Ok(Mapper.Map<PagedResultVm<MemberVm>>(result));
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
