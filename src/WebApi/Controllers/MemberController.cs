using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
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
        [ServiceFilter(typeof(ValidateMemberDtoFilter))]
        public async Task<IActionResult> AddMember([FromBody] MemberDto memberDto)
        {
            try
            {
                var entityMember = _mapper.Map<EntityMember>(memberDto);
                await _memberService.AddEntityMemberAsync(entityMember);
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
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return NotFound(_localizer["MemberIdNotFound"]);
            }
            var memberDto = _mapper.Map<MemberDto>(member);
            return Ok(memberDto);
        }
    }
}