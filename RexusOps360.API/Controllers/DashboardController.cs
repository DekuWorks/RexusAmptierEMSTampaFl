using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Data;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var stats = InMemoryStore.GetDashboardStats();
            return Ok(stats);
        }
    }
} 