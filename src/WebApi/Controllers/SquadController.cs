using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Stellantis.ProjectName.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SquadController : ControllerBase
    {
        private readonly ISquadService _squadService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SquadResources> _localizer;

        public SquadController(ISquadService squadService, IMapper mapper, IStringLocalizer<SquadResources> localizer)
        {
            _squadService = squadService;
            _mapper = mapper;
            _localizer = localizer;
        }

        // AMS-53: Create a new squad
        [HttpPost]
        public IActionResult CreateSquad([FromBody] CreateSquadRequest request)
        {
            try
            {
                _squadService.CreateSquad(request.Name, request.Description);
                return Ok(new { Message = _localizer[nameof(SquadResources.SquadCreatedSuccessfully)] });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
        }

        // AMS-54: Get squad details by ID
        [HttpGet("{id}")]
        public IActionResult GetSquadById(Guid id)
        {
            try
            {
                var squad = _squadService.GetSquadById(id);
                var squadDto = _mapper.Map<SquadDto>(squad);
                return Ok(squadDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // AMS-55: Update an existing squad
        [HttpPut("{id}")]
        public IActionResult UpdateSquad(Guid id, [FromBody] UpdateSquadRequest request)
        {
            try
            {
                _squadService.UpdateSquad(id, request.Name, request.Description);
                return Ok(new { Message = _localizer[nameof(SquadResources.SquadUpdatedSuccessfully)] });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
        }

        // AMS-56: List all squads
        [HttpGet]
        public IActionResult GetAllSquads([FromQuery] string name = null)
        {
            var squads = _squadService.GetAllSquads(name);
            var squadDtos = _mapper.Map<IEnumerable<SquadDto>>(squads);
            return Ok(squadDtos);
        }

        // AMS-57: Delete a squad
        [HttpDelete("{id}")]
        public IActionResult DeleteSquad(Guid id)
        {
            try
            {
                _squadService.DeleteSquad(id);
                return Ok(new { Message = _localizer[nameof(SquadResources.SquadSuccessfullyDeleted)] });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }

    public class CreateSquadRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UpdateSquadRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class SquadDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
