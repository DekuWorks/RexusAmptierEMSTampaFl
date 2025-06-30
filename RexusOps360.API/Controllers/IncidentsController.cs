using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Data;
using RexusOps360.API.Models;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IncidentsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            var incidents = InMemoryStore.GetAllIncidents();
            return Ok(new
            {
                incidents = incidents,
                count = incidents.Count
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var incident = InMemoryStore.GetIncidentById(id);
            if (incident == null)
                return NotFound(new { error = "Incident not found" });

            return Ok(incident);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Dispatcher")]
        public IActionResult Create([FromBody] Incident incident)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Invalid data provided" });

            var createdIncident = InMemoryStore.CreateIncident(incident);
            return CreatedAtAction(nameof(GetById), new { id = createdIncident.Id }, new
            {
                message = "Incident created successfully",
                incident = createdIncident
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public IActionResult Update(int id, [FromBody] Incident updatedIncident)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Invalid data provided" });

            var existingIncident = InMemoryStore.UpdateIncident(id, updatedIncident);
            if (existingIncident == null)
                return NotFound(new { error = "Incident not found" });

            return Ok(new
            {
                message = "Incident updated successfully",
                incident = existingIncident
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var deleted = InMemoryStore.DeleteIncident(id);
            if (!deleted)
                return NotFound(new { error = "Incident not found" });

            return Ok(new { message = "Incident deleted successfully" });
        }
    }
} 