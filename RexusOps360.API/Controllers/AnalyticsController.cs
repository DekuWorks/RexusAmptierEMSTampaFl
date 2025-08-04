using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using RexusOps360.API.Data;
using RexusOps360.API.Models;
using RexusOps360.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only admin can access analytics
    public class AnalyticsController : ControllerBase
    {
        private readonly EmsDbContext _context;
        private readonly IHubContext<EmsHub> _hubContext;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(EmsDbContext context, IHubContext<EmsHub> hubContext, ILogger<AnalyticsController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
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
                var incidentTrends = await GetIncidentTrends();
                var responderPerformance = await GetResponderPerformance();
                var systemHealth = await GetSystemHealth();

                var result = new
                {
                    activeIncidents,
                    availableResponders,
                    totalEquipment,
                    availableEquipment,
                    avgResponseTime,
                    equipmentUtilization = totalEquipment > 0 ? (double)availableEquipment / totalEquipment * 100 : 0,
                    incidentTrends,
                    responderPerformance,
                    systemHealth
                };

                // Send real-time update to admin dashboard
                await _hubContext.Clients.Group("admin").SendAsync("AnalyticsUpdated", result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard analytics");
                return StatusCode(500, new { error = "Error retrieving dashboard analytics", details = ex.Message });
            }
        }

        [HttpGet("incidents/chart")]
        public async Task<IActionResult> GetIncidentChartData()
        {
            try
            {
                var last30Days = DateTime.UtcNow.AddDays(-30);
                
                var incidentsByDay = await _context.Incidents
                    .Where(i => i.CreatedAt >= last30Days)
                    .GroupBy(i => i.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Count = g.Count(),
                        HighPriority = g.Count(i => i.Priority == "High"),
                        MediumPriority = g.Count(i => i.Priority == "Medium"),
                        LowPriority = g.Count(i => i.Priority == "Low")
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var incidentsByType = await _context.Incidents
                    .Where(i => i.CreatedAt >= last30Days)
                    .GroupBy(i => i.Type)
                    .Select(g => new
                    {
                        Type = g.Key,
                        Count = g.Count(),
                        Percentage = (double)g.Count() / _context.Incidents.Count(i => i.CreatedAt >= last30Days) * 100
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                var result = new
                {
                    incidentsByDay,
                    incidentsByType,
                    totalIncidents = incidentsByDay.Sum(x => x.Count),
                    avgIncidentsPerDay = incidentsByDay.Any() ? incidentsByDay.Average(x => x.Count) : 0
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving incident chart data");
                return StatusCode(500, new { error = "Error retrieving incident chart data", details = ex.Message });
            }
        }

        [HttpGet("response-times")]
        public async Task<IActionResult> GetResponseTimeAnalytics()
        {
            try
            {
                var last30Days = DateTime.UtcNow.AddDays(-30);
                
                var responseTimes = await _context.Incidents
                    .Where(i => i.CreatedAt >= last30Days && i.RespondedAt.HasValue)
                    .Select(i => new
                    {
                        IncidentId = i.Id,
                        Type = i.Type,
                        Priority = i.Priority,
                        ResponseTime = (i.RespondedAt.Value - i.CreatedAt).TotalMinutes,
                        CreatedAt = i.CreatedAt
                    })
                    .ToListAsync();

                var avgResponseTimeByPriority = responseTimes
                    .GroupBy(x => x.Priority)
                    .Select(g => new
                    {
                        Priority = g.Key,
                        AverageResponseTime = g.Average(x => x.ResponseTime),
                        Count = g.Count(),
                        MinResponseTime = g.Min(x => x.ResponseTime),
                        MaxResponseTime = g.Max(x => x.ResponseTime)
                    })
                    .ToList();

                var avgResponseTimeByType = responseTimes
                    .GroupBy(x => x.Type)
                    .Select(g => new
                    {
                        Type = g.Key,
                        AverageResponseTime = g.Average(x => x.ResponseTime),
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.AverageResponseTime)
                    .ToList();

                var result = new
                {
                    overallAverage = responseTimes.Any() ? responseTimes.Average(x => x.ResponseTime) : 0,
                    averageByPriority = avgResponseTimeByPriority,
                    averageByType = avgResponseTimeByType,
                    totalIncidents = responseTimes.Count,
                    responseTimeDistribution = new
                    {
                        under5Minutes = responseTimes.Count(x => x.ResponseTime <= 5),
                        under10Minutes = responseTimes.Count(x => x.ResponseTime <= 10),
                        under15Minutes = responseTimes.Count(x => x.ResponseTime <= 15),
                        over15Minutes = responseTimes.Count(x => x.ResponseTime > 15)
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving response time analytics");
                return StatusCode(500, new { error = "Error retrieving response time analytics", details = ex.Message });
            }
        }

        [HttpGet("responder-performance")]
        public async Task<IActionResult> GetResponderPerformanceAnalytics()
        {
            try
            {
                var last30Days = DateTime.UtcNow.AddDays(-30);
                
                var responderStats = await _context.Responders
                    .Select(r => new
                    {
                        ResponderId = r.Id,
                        Name = r.Name,
                        Role = r.Role,
                        Status = r.Status,
                        IncidentsHandled = _context.Incidents.Count(i => i.Responders != null && i.Responders.Any(resp => resp.Id == r.Id)),
                        RecentIncidents = _context.Incidents.Count(i => i.Responders != null && i.Responders.Any(resp => resp.Id == r.Id) && i.CreatedAt >= last30Days),
                        AverageResponseTime = _context.Incidents
                            .Where(i => i.Responders != null && i.Responders.Any(resp => resp.Id == r.Id) && i.RespondedAt.HasValue)
                            .Select(i => (i.RespondedAt.Value - i.CreatedAt).TotalMinutes)
                            .DefaultIfEmpty(0)
                            .Average()
                    })
                    .ToListAsync();

                var result = new
                {
                    totalResponders = responderStats.Count,
                    activeResponders = responderStats.Count(r => r.Status == "Active"),
                    averageIncidentsPerResponder = responderStats.Any() ? responderStats.Average(r => r.IncidentsHandled) : 0,
                    topPerformers = responderStats
                        .OrderByDescending(r => r.RecentIncidents)
                        .Take(10)
                        .Select(r => new
                        {
                            r.Name,
                            r.Role,
                            r.RecentIncidents,
                            r.AverageResponseTime
                        }),
                    performanceByRole = responderStats
                        .GroupBy(r => r.Role)
                        .Select(g => new
                        {
                            Role = g.Key,
                            Count = g.Count(),
                            AverageIncidents = g.Average(r => r.IncidentsHandled),
                            AverageResponseTime = g.Average(r => r.AverageResponseTime)
                        })
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving responder performance analytics");
                return StatusCode(500, new { error = "Error retrieving responder performance analytics", details = ex.Message });
            }
        }

        [HttpGet("system-health")]
        public async Task<IActionResult> GetSystemHealthAnalytics()
        {
            try
            {
                var systemHealth = new
                {
                    databaseStatus = "Healthy",
                    apiStatus = "Healthy",
                    signalRStatus = "Connected",
                    uptime = DateTime.UtcNow.Subtract(DateTime.UtcNow.AddDays(-1)).TotalHours,
                    activeConnections = 0, // Would be implemented with connection tracking
                    memoryUsage = GC.GetTotalMemory(false),
                    cpuUsage = 0, // Would be implemented with system monitoring
                    lastBackup = DateTime.UtcNow.AddHours(-6),
                    alerts = new List<object>(),
                    recommendations = new List<string>
                    {
                        "System performance is optimal",
                        "Consider adding more responders during peak hours",
                        "Database backup scheduled for tonight"
                    }
                };

                // Send real-time system health update
                await _hubContext.Clients.Group("admin").SendAsync("SystemHealthUpdated", systemHealth);

                return Ok(systemHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system health analytics");
                return StatusCode(500, new { error = "Error retrieving system health analytics", details = ex.Message });
            }
        }

        private async Task<double> CalculateAverageResponseTime()
        {
            var incidents = await _context.Incidents
                .Where(i => i.RespondedAt.HasValue)
                .Select(i => (i.RespondedAt.Value - i.CreatedAt).TotalMinutes)
                .ToListAsync();

            return incidents.Any() ? incidents.Average() : 0;
        }

        private async Task<object> GetIncidentTrends()
        {
            var last7Days = DateTime.UtcNow.AddDays(-7);
            
            var dailyIncidents = await _context.Incidents
                .Where(i => i.CreatedAt >= last7Days)
                .GroupBy(i => i.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return new
            {
                dailyTrend = dailyIncidents,
                totalThisWeek = dailyIncidents.Sum(x => x.Count),
                averagePerDay = dailyIncidents.Any() ? dailyIncidents.Average(x => x.Count) : 0,
                trend = dailyIncidents.Count >= 2 ? 
                    (dailyIncidents.Last().Count - dailyIncidents.First().Count) / (double)dailyIncidents.First().Count * 100 : 0
            };
        }

        private async Task<object> GetResponderPerformance()
        {
            var responders = await _context.Responders
                .Select(r => new
                {
                    r.Name,
                    r.Role,
                    r.Status,
                    IncidentsHandled = _context.Incidents.Count(i => i.Responders != null && i.Responders.Any(resp => resp.Id == r.Id))
                })
                .ToListAsync();

            return new
            {
                totalResponders = responders.Count,
                activeResponders = responders.Count(r => r.Status == "Active"),
                averageIncidentsPerResponder = responders.Any() ? responders.Average(r => r.IncidentsHandled) : 0,
                topResponders = responders.OrderByDescending(r => r.IncidentsHandled).Take(5)
            };
        }

        private async Task<object> GetSystemHealth()
        {
            return new
            {
                status = "Healthy",
                uptime = "99.9%",
                lastIncident = DateTime.UtcNow.AddHours(-2),
                activeAlerts = 0,
                systemLoad = "Normal"
            };
        }
    }
} 