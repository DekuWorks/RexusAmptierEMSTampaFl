using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Data;
using RexusOps360.API.Models;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RespondersController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            var responders = InMemoryStore.GetAllResponders();
            return Ok(new
            {
                responders = responders,
                count = responders.Count
            });
        }

        [HttpPost]
        public IActionResult Create([FromBody] Responder responder)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Invalid data provided" });

            var createdResponder = InMemoryStore.CreateResponder(responder);
            return CreatedAtAction(nameof(GetAll), new
            {
                message = "Responder created successfully",
                responder = createdResponder
            });
        }
    }
} 