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
    public abstract class EntityControllerBase<TEntity, TEntityDto>(IEntityServiceBase<TEntity> service, IMapper mapper, IStringLocalizerFactory localizerFactory) : ControllerBase
        where TEntity : EntityBase
        where TEntityDto : class
    {
        protected virtual IEntityServiceBase<TEntity> Service { get; } = service ?? throw new ArgumentNullException(nameof(service));
        protected IMapper Mapper { get; } = mapper ?? throw new ArgumentNullException(nameof(mapper));
        protected IStringLocalizer Localizer { get; } = localizerFactory.Create(typeof(ControllerResources));

        protected async Task<IActionResult> CreateBaseAsync<TEntityVm>(TEntityDto itemDto) where TEntityVm : EntityVmBase
        {
            if (itemDto == null)
            {
                return BadRequest(ErrorResponse.BadRequest(Localizer[nameof(ControllerResources.CannotBeNull)]));
            }

            TEntity item = Mapper.Map<TEntity>(itemDto);
            OperationResult result = await Service.CreateAsync(item!);

            return result.Status switch
            {
                OperationStatus.Success => CreatedAtAction(HttpMethod.Get.Method, new { id = item!.Id }, Mapper.Map<TEntityVm>(item)),
                OperationStatus.Conflict => Conflict(result),
                OperationStatus.InvalidData => UnprocessableEntity(result),
                _ => BadRequest(result)
            };
        }

        protected async Task<ActionResult> GetAsync<TEntityVm>(int id) where TEntityVm : EntityVmBase
        {
            TEntity? item = await Service.GetItemAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            TEntityVm result = Mapper.Map<TEntityVm>(item);
            return Ok(result);
        }

        protected async Task<IActionResult> UpdateBaseAsync<TEntityVm>(int id, TEntityDto itemDto) where TEntityVm : EntityVmBase
        {
            if (itemDto == null)
            {
                return BadRequest(ErrorResponse.BadRequest(Localizer[nameof(ControllerResources.CannotBeNull)]));
            }

            TEntity item = Mapper.Map<TEntity>(itemDto);
            item!.Id = id;
            OperationResult result = await Service.UpdateAsync(item);

            return result.Status switch
            {
                OperationStatus.Success => Ok(Mapper.Map<TEntityVm>(item)),
                OperationStatus.Conflict => Conflict(result),
                OperationStatus.NotFound => NotFound(),
                OperationStatus.InvalidData => UnprocessableEntity(result),
                _ => BadRequest(result)
            };
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync(int id)
        {
            OperationResult result = await Service.DeleteAsync(id);
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
