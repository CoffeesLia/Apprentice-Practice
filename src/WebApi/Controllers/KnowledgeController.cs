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
    [Route("api/knowledges")]
    public sealed class KnowledgeController(IKnowledgeService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Knowledge, KnowledgeDto>(service, mapper, localizerFactory)
    {
        protected override IKnowledgeService Service => (IKnowledgeService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] KnowledgeDto itemDto)
        {
            return await CreateBaseAsync<KnowledgeVm>(itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<KnowledgeVm>> GetAsync(int id)
        {
            return await GetAsync<KnowledgeVm>(id).ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] KnowledgeDto itemDto)
        {
            return await UpdateBaseAsync<KnowledgeVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            IActionResult result = await base.DeleteAsync(id).ConfigureAwait(false);
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] KnowledgeFilterDto filterDto)
        {
            KnowledgeFilter filter = Mapper.Map<KnowledgeFilter>(filterDto);
            PagedResult<Knowledge> pagedResult = await Service.GetListAsync(filter!).ConfigureAwait(false);
            PagedResultVm<KnowledgeVm> result = Mapper.Map<PagedResultVm<KnowledgeVm>>(pagedResult);
            return Ok(result);
        }

        // Métodos customizados

        // Cria uma associação entre um membro e uma aplicação (registra que o membro conhece aquela aplicação)
        [HttpPost("associate")]
        public async Task<IActionResult> AssociateAsync([FromBody] KnowledgeDto dto)
        {
            await Service.CreateAssociationAsync(dto.MemberId, dto.ApplicationId, dto.CurrentSquadId).ConfigureAwait(false);
            return NoContent();
        }

        // Remove a associação entre um membro e uma aplicação (indica que o membro não conhece mais aquela aplicação).
        [HttpPost("remove-association")]
        public async Task<IActionResult> RemoveAssociationAsync([FromBody] KnowledgeDto dto)
        {
            await Service.RemoveAssociationAsync(dto.MemberId, dto.ApplicationId, dto.LeaderSquadId).ConfigureAwait(false);
            return NoContent();
        }

        // Lista todas as aplicações que um determinado membro conhece.
        [HttpGet("applications-by-member/{memberId:int}")]
        public async Task<ActionResult<List<ApplicationData>>> ListApplicationsByMemberAsync(int memberId)
        {
            var result = await Service.ListApplicationsByMemberAsync(memberId).ConfigureAwait(false);
            return Ok(result);
        }

        // Lista todos os membros que conhecem uma determinada aplicação.
        [HttpGet("members-by-application/{applicationId:int}")]
        public async Task<ActionResult<List<Member>>> ListMembersByApplicationAsync(int applicationId)
        {
            var result = await Service.ListMembersByApplicationAsync(applicationId).ConfigureAwait(false);
            return Ok(result);
        }
    }
}
