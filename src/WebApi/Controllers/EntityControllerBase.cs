using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Resources;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class EntityControllerBase<TEntity, TEntityDto> : ControllerBase
        where TEntity : EntityBase
        where TEntityDto : class
    {
        protected virtual IEntityServiceBase<TEntity> Service { get; }
        protected IMapper Mapper { get; }
        protected IStringLocalizer Localizer { get; }

        protected EntityControllerBase(IEntityServiceBase<TEntity> service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            Localizer = localizerFactory.Create(typeof(ControllerResources));
        }

        protected async Task<IActionResult> CreateBaseAsync<TEntityVm>(TEntityDto itemDto) where TEntityVm : EntityVmBase
        {
            if (itemDto == null)
                return BadRequest(new { Message = Localizer["InvalidAreaData"] });

            var item = Mapper.Map<TEntity>(itemDto);
            var result = await Service.CreateAsync(item!);

            return result.Status switch
            {
                OperationStatus.Success => CreatedAtAction(nameof(GetAsync), new { id = item!.Id }, Mapper.Map<TEntityVm>(item)),
                OperationStatus.Conflict => Conflict(result),
                OperationStatus.InvalidData => UnprocessableEntity(result),
                _ => BadRequest(result)
            };
        }

        protected async Task<IActionResult> UpdateBaseAsync<TEntityVm>(int id, TEntityDto itemDto) where TEntityVm : EntityVmBase
        {
            if (itemDto == null)
                return BadRequest(new { Message = Localizer["InvalidAreaData"] });

            var item = Mapper.Map<TEntity>(itemDto);
            item!.Id = id;
            var result = await Service.UpdateAsync(item);

            return result.Status switch
            {
                OperationStatus.Success => Ok(Mapper.Map<TEntityVm>(item)),
                OperationStatus.Conflict => Conflict(result),
                OperationStatus.NotFound => NotFound(),
                OperationStatus.InvalidData => UnprocessableEntity(result),
                _ => BadRequest(result)
            };
        }

        protected async Task<ActionResult> GetAsync<TEntityVm>(int id) where TEntityVm : EntityVmBase
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
            return result.Status switch
            {
                OperationStatus.Success => NoContent(),
                OperationStatus.Conflict => Conflict(result),
                OperationStatus.NotFound => NotFound(),
                _ => BadRequest(result)
            };
        }
    }
}
