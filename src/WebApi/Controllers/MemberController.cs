using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly IStringLocalizer<ServiceResources> _localizer;
        public MemberController(IMemberService memberService, IStringLocalizer<ServiceResources> localizer)
        {
            _memberService = memberService;
            _localizer = localizer;
        }
        [HttpPost]
        public IActionResult AddMember([FromBody] EntityMember entityMember)
        {
            try
            {
                _memberService.AddEntityMember(entityMember);
                return Ok(_localizer["MemberAddedSuccessfully"]);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
