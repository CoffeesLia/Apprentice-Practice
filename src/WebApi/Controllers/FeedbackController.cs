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
    [ApiController]
    [Route("api/feedbacks")]
    public class FeedbackController(IFeedbackService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Feedback, FeedbackDto>(service, mapper, localizerFactory)
    {
        protected override IFeedbackService Service => (IFeedbackService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] FeedbackDto itemDto)
        {
            return await CreateBaseAsync<FeedbackVm>(itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FeedbackVm>> GetAsync(int id)
        {
            return await GetAsync<FeedbackVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] FeedbackFilterDto filterDto)
        {
            FeedbackFilter filter = Mapper.Map<FeedbackFilter>(filterDto);
            PagedResult<Feedback> result = await Service.GetListAsync(filter).ConfigureAwait(false);
            return Ok(Mapper.Map<PagedResultVm<FeedbackVm>>(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] FeedbackDto itemDto)
        {
            return await base.UpdateBaseAsync<FeedbackVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

    }
}