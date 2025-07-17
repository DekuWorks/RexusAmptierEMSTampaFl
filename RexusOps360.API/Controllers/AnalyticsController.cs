using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RexusOps360.API.Data;
using RexusOps360.API.Models;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly EmsDbContext _context;

        public AnalyticsController(EmsDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardAnalytics()
        {
            try
            {
                var activeIncidents = await _context.Incidents
                    .Where(i => i.Status == "Active" || i.Status == "In Progress")
                    .CountAsync();

                var availableResponders = await _context.Responders
                    .Where(r => r.Status == "Available")
                    .CountAsync();

                var totalEquipment = await _context.Equipment.CountAsync();
                var availableEquipment = await _context.Equipment
                    .Where(e => e.Status == "Available")
                    .CountAsync();

                var avgResponseTime = await CalculateAverageResponseTime();

                var result = new
                {
                    activeIncidents,
                    availableResponders,
                    totalEquipment,
                    availableEquipment,
                    avgResponseTime,
                    equipmentUtilization = totalEquipment > 0 ? (double)availableEquipment / totalEquipment * 100 : 0
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error retrieving dashboard analytics", details = ex.Message });
            }
        }

        [HttpGet("incidents/chart")]
        public async Task<IActionResult> GetIncidentChartData()
        {
            try
            {
                var incidentData = await _context.Incidents
                    .GroupBy(i => i.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var labels = new List<string>();
                var data = new List<int>();

                foreach (var item in incidentData)
                {
                    labels.Add(item.Status);
                    data.Add(item.Count);
                }

                return Ok(new { labels, data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error retrieving incident chart data", details = ex.Message });
            }
        }

        [HttpGet("responders/chart")]
        public async Task<IActionResult> GetResponderChartData()
        {
            try
            {
                var responderData = await _context.Responders
                    .GroupBy(r => r.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var labels = new List<string>();
                var data = new List<int>();

                foreach (var item in responderData)
                {
                    labels.Add(item.Status);
                    data.Add(item.Count);
                }

                return Ok(new { labels, data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error retrieving responder chart data", details = ex.Message });
            }
        }

        [HttpGet("equipment/chart")]
        public async Task<IActionResult> GetEquipmentChartData()
        {
            try
            {
                var equipmentData = await _context.Equipment
                    .GroupBy(e => e.Type)
                    .Select(g => new { Type = g.Key, Available = g.Sum(e => e.AvailableQuantity), Total = g.Sum(e => e.Quantity) })
                    .ToListAsync();

                var labels = new List<string>();
                var availableData = new List<int>();
                var totalData = new List<int>();

                foreach (var item in equipmentData)
                {
                    labels.Add(item.Type);
                    availableData.Add(item.Available);
                    totalData.Add(item.Total);
                }

                return Ok(new { labels, availableData, totalData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error retrieving equipment chart data", details = ex.Message });
            }
        }

        [HttpGet("response-times")]
        public async Task<IActionResult> GetResponseTimeData()
        {
            try
            {
                var responseTimeData = await _context.Incidents
                    .Where(i => i.Status == "Resolved" && i.UpdatedAt.HasValue)
                    .GroupBy(i => i.Type)
                    .Select(g => new 
                    { 
                        Type = g.Key, 
                        AvgResponseTime = g.Average(i => i.UpdatedAt.HasValue ? (i.UpdatedAt.Value - i.CreatedAt).TotalMinutes : 0)
                    })
                    .ToListAsync();

                var labels = new List<string>();
                var data = new List<double>();

                foreach (var item in responseTimeData)
                {
                    labels.Add(item.Type);
                    data.Add(Math.Round(item.AvgResponseTime, 1));
                }

                return Ok(new { labels, data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error retrieving response time data", details = ex.Message });
            }
        }

        [HttpGet("trends")]
        public async Task<IActionResult> GetTrendsData([FromQuery] int days = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);

                var dailyIncidents = await _context.Incidents
                    .Where(i => i.CreatedAt >= startDate)
                    .GroupBy(i => i.CreatedAt.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var labels = dailyIncidents.Select(x => x.Date.ToString("MM/dd")).ToList();
                var data = dailyIncidents.Select(x => x.Count).ToList();

                return Ok(new { labels, data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error retrieving trends data", details = ex.Message });
            }
        }

        private async Task<double> CalculateAverageResponseTime()
        {
            var resolvedIncidents = await _context.Incidents
                .Where(i => i.Status == "Resolved" && i.UpdatedAt.HasValue)
                .ToListAsync();

            if (!resolvedIncidents.Any())
                return 0;

            var totalResponseTime = resolvedIncidents.Sum(i => i.UpdatedAt.HasValue ? (i.UpdatedAt.Value - i.CreatedAt).TotalMinutes : 0);
            return Math.Round(totalResponseTime / resolvedIncidents.Count, 1);
        }
    }
} 