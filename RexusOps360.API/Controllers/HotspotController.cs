using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Models;
using RexusOps360.API.Services;
using Microsoft.EntityFrameworkCore;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Dispatcher")]
    public class HotspotController : ControllerBase
    {
        private readonly IHotspotDetectionService _hotspotService;
        private readonly ILogger<HotspotController> _logger;

        public HotspotController(
            IHotspotDetectionService hotspotService,
            ILogger<HotspotController> logger)
        {
            _hotspotService = hotspotService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetHotspots([FromQuery] string? utilityType = null)
        {
            try
            {
                var hotspots = await _hotspotService.GetActiveHotspotsAsync(utilityType);
                
                return Ok(new
                {
                    hotspots = hotspots,
                    count = hotspots.Count,
                    critical_count = hotspots.Count(h => h.Severity == "Critical"),
                    high_count = hotspots.Count(h => h.Severity == "High"),
                    medium_count = hotspots.Count(h => h.Severity == "Medium"),
                    low_count = hotspots.Count(h => h.Severity == "Low")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotspots");
                return StatusCode(500, new { error = "Error retrieving hotspots" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotspot(int id)
        {
            try
            {
                var hotspots = await _hotspotService.GetActiveHotspotsAsync();
                var hotspot = hotspots.FirstOrDefault(h => h.Id == id);
                
                if (hotspot == null)
                    return NotFound(new { error = "Hotspot not found" });

                return Ok(new { hotspot = hotspot });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotspot {HotspotId}", id);
                return StatusCode(500, new { error = "Error retrieving hotspot" });
            }
        }

        [HttpPost("detect")]
        public async Task<IActionResult> DetectHotspots([FromQuery] string? utilityType = null)
        {
            try
            {
                var hotspots = await _hotspotService.DetectHotspotsAsync(utilityType);
                
                return Ok(new
                {
                    message = "Hotspot detection completed",
                    hotspots_detected = hotspots.Count,
                    hotspots = hotspots,
                    detection_time = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting hotspots");
                return StatusCode(500, new { error = "Error detecting hotspots" });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateHotspotStatus(int id, [FromBody] UpdateHotspotStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { error = "Invalid data provided" });

                var success = await _hotspotService.UpdateHotspotStatusAsync(id, request.Status);
                
                if (!success)
                    return NotFound(new { error = "Hotspot not found" });

                return Ok(new
                {
                    message = "Hotspot status updated successfully",
                    hotspot_id = id,
                    new_status = request.Status,
                    updated_at = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotspot status for {HotspotId}", id);
                return StatusCode(500, new { error = "Error updating hotspot status" });
            }
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetHotspotAnalytics()
        {
            try
            {
                var analytics = await _hotspotService.GetHotspotAnalyticsAsync();
                
                return Ok(new
                {
                    analytics = analytics,
                    generated_at = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotspot analytics");
                return StatusCode(500, new { error = "Error retrieving hotspot analytics" });
            }
        }

        [HttpGet("utility/{utilityType}")]
        public Task<IActionResult> GetHotspotsByUtility(string utilityType)
        {
            try
            {
                var hotspots = _hotspotService.GetActiveHotspotsAsync(utilityType).Result;
                
                var analytics = new
                {
                    utility_type = utilityType,
                    total_hotspots = hotspots.Count,
                    critical_hotspots = hotspots.Count(h => h.Severity == "Critical"),
                    high_hotspots = hotspots.Count(h => h.Severity == "High"),
                    medium_hotspots = hotspots.Count(h => h.Severity == "Medium"),
                    low_hotspots = hotspots.Count(h => h.Severity == "Low"),
                    average_incident_count = hotspots.Any() ? hotspots.Average(h => h.IncidentCount) : 0,
                    average_duration_minutes = hotspots.Any() ? hotspots.Average(h => h.Duration.TotalMinutes) : 0
                };

                return Task.FromResult<IActionResult>(Ok(new
                {
                    analytics = analytics,
                    hotspots = hotspots
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotspots for utility type {UtilityType}", utilityType);
                return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Error retrieving hotspots" }));
            }
        }

        [HttpGet("severity/{severity}")]
        public Task<IActionResult> GetHotspotsBySeverity(string severity)
        {
            try
            {
                var allHotspots = _hotspotService.GetActiveHotspotsAsync().Result;
                var hotspots = allHotspots.Where(h => h.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase)).ToList();

                var analytics = new
                {
                    severity = severity,
                    total_hotspots = hotspots.Count,
                    average_incident_count = hotspots.Any() ? hotspots.Average(h => h.IncidentCount) : 0,
                    average_duration_minutes = hotspots.Any() ? hotspots.Average(h => h.Duration.TotalMinutes) : 0,
                    utility_distribution = hotspots.GroupBy(h => h.UtilityType)
                        .Select(g => new { utility_type = g.Key, count = g.Count() })
                };

                return Task.FromResult<IActionResult>(Ok(new
                {
                    analytics = analytics,
                    hotspots = hotspots
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotspots by severity {Severity}", severity);
                return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Error retrieving hotspots" }));
            }
        }

        [HttpGet("alerts")]
        public Task<IActionResult> GetHotspotAlerts([FromQuery] string? status = null)
        {
            try
            {
                // This would typically come from a separate alerts service
                // For now, we'll return a mock response
                var alerts = new List<object>
                {
                    new
                    {
                        id = 1,
                        hotspot_id = 1,
                        title = "Critical Hotspot Detected",
                        message = "A critical priority hotspot has been detected in Downtown Tampa. This area has experienced 8 incidents in the last 2 hours.",
                        alert_level = "Critical",
                        status = "Active",
                        created_at = DateTime.UtcNow.AddMinutes(-30)
                    }
                };

                if (!string.IsNullOrEmpty(status))
                {
                    alerts = alerts.Where(a => ((dynamic)a).status == status).ToList();
                }

                return Task.FromResult<IActionResult>(Ok(new
                {
                    alerts = alerts,
                    count = alerts.Count,
                    active_count = alerts.Count(a => ((dynamic)a).status == "Active")
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotspot alerts");
                return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Error retrieving hotspot alerts" }));
            }
        }

        [HttpPost("alerts/{alertId}/acknowledge")]
        public Task<IActionResult> AcknowledgeAlert(int alertId, [FromBody] AcknowledgeAlertRequest request)
        {
            try
            {
                // This would typically update the alert status in the database
                // For now, we'll return a mock response
                return Task.FromResult<IActionResult>(Ok(new
                {
                    message = "Alert acknowledged successfully",
                    alert_id = alertId,
                    acknowledged_by = request.AcknowledgedBy,
                    acknowledged_at = DateTime.UtcNow
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging alert {AlertId}", alertId);
                return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Error acknowledging alert" }));
            }
        }

        [HttpGet("thresholds")]
        public IActionResult GetHotspotThresholds()
        {
            var thresholds = new
            {
                incident_count = new
                {
                    low = 3,
                    medium = 5,
                    high = 8,
                    critical = 10
                },
                time_window_minutes = new
                {
                    default_value = 120,
                    minimum = 30,
                    maximum = 480
                },
                severity_levels = new
                {
                    low = 1,
                    medium = 2,
                    high = 3,
                    critical = 4
                }
            };

            return Ok(new { thresholds = thresholds });
        }
    }

    public class UpdateHotspotStatusRequest
    {
        public string Status { get; set; } = string.Empty; // "Active", "Resolved", "Monitoring"
    }

    public class AcknowledgeAlertRequest
    {
        public string AcknowledgedBy { get; set; } = string.Empty;
    }
} 