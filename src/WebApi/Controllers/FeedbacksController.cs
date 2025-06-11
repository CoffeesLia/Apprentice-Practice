using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/feedbacks")]
    public class FeedbacksController(IFeedbacksService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Feedbacks, FeedbacksDto>(service, mapper, localizerFactory)
    {
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] FeedbacksDto itemDto)
        {
            return await CreateBaseAsync<FeedbacksVm>(itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FeedbacksVm>> GetAsync(int id)
        {
            return await GetAsync<FeedbacksVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] FeedbacksFilterDto filterDto)
        {
            FeedbacksFilter filter = Mapper.Map<FeedbacksFilter>(filterDto);
            PagedResult<Feedbacks> pagedResult = await ((IFeedbacksService)Service).GetListAsync(filter!).ConfigureAwait(false);
            PagedResultVm<FeedbacksVm> result = Mapper.Map<PagedResultVm<FeedbacksVm>>(pagedResult);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] FeedbacksDto itemDto)
        {
            return await base.UpdateBaseAsync<FeedbacksVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

    }
}