using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Resources;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class EntityControllerBase<TEntityDto, TEntityVm, TEntity>(IMapper mapper, IEntityServiceBase<TEntity> service, IStringLocalizerFactory localizerFactory)
        : ControllerBase
        where TEntityDto : class
        where TEntityVm : EntityVmBase
        where TEntity : EntityBase
    {
        protected virtual IEntityServiceBase<TEntity> Service { get; } = service;
        protected IMapper Mapper { get; } = mapper;
        protected IStringLocalizer Localizer { get; } = localizerFactory.Create(typeof(ControllerResources));

        [HttpPost]
        public virtual async Task<IActionResult> CreateAsync([FromBody] TEntityDto itemDto)
        {
            if (itemDto == null)
                return BadRequest(ErrorResponse.BadRequest(Localizer[nameof(ControllerResources.CannotBeNull)]));

            var item = Mapper.Map<TEntity>(itemDto);
            var result = await Service.CreateAsync(item!);

            if (result.Success)
            {
                var createdItem = Mapper.Map<TEntityVm>(item);
                return CreatedAtAction(HttpMethod.Get.Method, new { id = createdItem!.Id }, createdItem);
            }
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> UpdateAsync(int id, [FromBody] TEntityDto itemDto)
        {
            if (itemDto == null)
                return BadRequest(ErrorResponse.BadRequest(Localizer[nameof(ControllerResources.CannotBeNull)]));

            var item = Mapper.Map<TEntity>(itemDto);
            var result = await Service.UpdateAsync(item!);

            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntityVm>> GetAsync(int id)
        {
            var item = await Service.GetItemAsync(id);

            if (item == null)
                return NotFound();

            var result = Mapper.Map<TEntityVm>(item);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await Service.DeleteAsync(id);
            if (result.Success)
                return NoContent();
            return BadRequest(result);// Review, porque deveria retorna not found quando não encontrado. Pensar em alterar o retorno do serviço ou 409 Conflict
        }
    }
}
