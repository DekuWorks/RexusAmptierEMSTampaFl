using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Data;
using RexusOps360.API.Models;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EquipmentController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            var equipment = InMemoryStore.GetAllEquipment();
            return Ok(new
            {
                equipment = equipment,
                count = equipment.Count
            });
        }

        [HttpPost]
        public IActionResult Create([FromBody] Equipment equipment)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Invalid data provided" });

            var createdEquipment = InMemoryStore.CreateEquipment(equipment);
            return CreatedAtAction(nameof(GetAll), new
            {
                message = "Equipment added successfully",
                equipment = createdEquipment
            });
        }
    }
} 