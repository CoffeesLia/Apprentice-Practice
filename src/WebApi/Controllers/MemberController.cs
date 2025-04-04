/*
using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.ViewModels;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using AutoMapper;
using Stellantis.ProjectName.WebApi.Filters;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly IStringLocalizer<ServiceResources> _localizer;
        private readonly IMapper _mapper;

        public MemberController(IMemberService memberService, IStringLocalizer<ServiceResources> localizer, IMapper mapper)
        {
            _memberService = memberService;
            _localizer = localizer;
            _mapper = mapper;
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidateMemberVmFilter))]
        public async Task<IActionResult> AddMember([FromBody] MemberVm memberVm)
        {
            try
            {
                var entityMember = _mapper.Map<EntityMember>(memberVm);
                entityMember.Id = Guid.NewGuid();
                await _memberService.AddEntityMemberAsync(entityMember).ConfigureAwait(false);
                return Ok(_localizer["MemberAddedSuccessfully"]);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberById(Guid id)
        {
            var member = await _memberService.GetMemberByIdAsync(id).ConfigureAwait(false);
            if (member == null)
            {
                return NotFound(_localizer["MemberIdNotFound"]);
            }
            var memberVm = _mapper.Map<MemberVm>(member);
            return Ok(memberVm);
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidateMemberVmFilter))]
        public async Task<IActionResult> UpdateMember(Guid id, [FromBody] MemberVm memberVm)
        {
            try
            {
                var entityMember = _mapper.Map<EntityMember>(memberVm);
                entityMember.Id = id;
                await _memberService.UpdateEntityMemberAsync(entityMember).ConfigureAwait(false);
                return Ok(_localizer["MemberUpdatedSuccessfully"]);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMembers([FromQuery] string? name, [FromQuery] string? email, [FromQuery] string? role)
        {
            var members = await _memberService.GetMembersAsync(name, email, role).ConfigureAwait(false);
            var membersVms = _mapper.Map<IEnumerable<MemberVm>>(members);
            return Ok(membersVms);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(Guid id)
        {
            try
            {
                await _memberService.DeleteMemberByIdAsync(id).ConfigureAwait(false);
                return Ok(_localizer["MemberDeletedSuccessfully"]);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
*/