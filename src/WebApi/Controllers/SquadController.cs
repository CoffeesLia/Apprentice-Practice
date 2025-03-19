using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Interfaces.Services;
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

        public SquadController(ISquadService squadService)
        {
            _squadService = squadService;
        }

        // AMS-53: Create a new squad
        [HttpPost]
        public IActionResult CreateSquad([FromBody] CreateSquadRequest request)
        {
            try
            {
                _squadService.CreateSquad(request.Name, request.Description);
                return Ok(new { Message = "Squad created successfully." });
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
                return Ok(squad);
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
                return Ok(new { Message = "Squad updated successfully." });
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
}
