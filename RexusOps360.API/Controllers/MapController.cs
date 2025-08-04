/*
 * RexusOps360 EMS API - Map Controller
 * 
 * This controller handles real-time map data, incident tracking,
 * and location updates for the EMS system.
 * 
 * Features:
 * - Real-time incident map data
 * - Location updates
 * - Status tracking
 * - Filtering by disaster type and severity
 * - Live statistics
 * 
 * Author: RexusOps360 Development Team
 * Version: 1.0.0
 * Last Updated: 2025-01-17
 */

using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Models;
using RexusOps360.API.Services;

namespace RexusOps360.API.Controllers
{
    /// <summary>
    /// Map Controller for Real-Time Incident Tracking
    /// Handles map data, location updates, and incident tracking
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly IRealTimeTrackingService _trackingService;
        private readonly ILogger<MapController> _logger;

        public MapController(
            IRealTimeTrackingService trackingService,
            ILogger<MapController> logger)
        {
            _trackingService = trackingService;
            _logger = logger;
        }

        /// <summary>
        /// Get real-time map data with incidents and responders
        /// </summary>
        /// <returns>Complete map data including incidents, responders, and statistics</returns>
        /// <response code="200">Map data retrieved successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("data")]
        public async Task<IActionResult> GetMapData()
        {
            try
            {
                var mapData = await _trackingService.GetMapDataAsync();
                
                _logger.LogInformation("Map data retrieved: {IncidentCount} incidents, {ResponderCount} responders", 
                    mapData.Incidents.Count, mapData.Responders.Count);
                
                return Ok(new ApiResponse<MapData>
                {
                    Success = true,
                    Data = mapData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving map data");
                return StatusCode(500, new ApiResponse<MapData>
                {
                    Success = false,
                    Message = "Error retrieving map data"
                });
            }
        }

        /// <summary>
        /// Get filtered incidents by type and severity
        /// </summary>
        /// <param name="type">Disaster type filter</param>
        /// <param name="priority">Priority level filter</param>
        /// <param name="status">Status filter</param>
        /// <returns>Filtered list of incidents</returns>
        /// <response code="200">Filtered incidents retrieved successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("incidents")]
        public async Task<IActionResult> GetFilteredIncidents(
            [FromQuery] string? type = null,
            [FromQuery] string? priority = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var incidents = await _trackingService.GetActiveIncidentsAsync();
                
                // Apply filters
                var filteredIncidents = incidents.AsQueryable();
                
                if (!string.IsNullOrEmpty(type))
                {
                    filteredIncidents = filteredIncidents.Where(i => i.Type == type);
                }
                
                if (!string.IsNullOrEmpty(priority))
                {
                    filteredIncidents = filteredIncidents.Where(i => i.Priority == priority);
                }
                
                if (!string.IsNullOrEmpty(status))
                {
                    filteredIncidents = filteredIncidents.Where(i => i.Status == status);
                }
                
                var result = filteredIncidents.ToList();
                
                _logger.LogInformation("Filtered incidents retrieved: {Count} incidents", result.Count);
                
                return Ok(new ApiResponse<List<Incident>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered incidents");
                return StatusCode(500, new ApiResponse<List<Incident>>
                {
                    Success = false,
                    Message = "Error retrieving filtered incidents"
                });
            }
        }

        /// <summary>
        /// Update incident location coordinates
        /// </summary>
        /// <param name="request">Location update request</param>
        /// <returns>Success response</returns>
        /// <response code="200">Location updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("incident/location")]
        public async Task<IActionResult> UpdateIncidentLocation([FromBody] LocationUpdateRequest request)
        {
            try
            {
                if (request == null || request.IncidentId <= 0)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }
                
                await _trackingService.UpdateIncidentLocationAsync(
                    request.IncidentId, 
                    request.Latitude, 
                    request.Longitude);
                
                _logger.LogInformation("Updated location for incident {IncidentId}: {Lat}, {Lng}", 
                    request.IncidentId, request.Latitude, request.Longitude);
                
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Incident location updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating incident location");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error updating incident location"
                });
            }
        }

        /// <summary>
        /// Update incident status
        /// </summary>
        /// <param name="request">Status update request</param>
        /// <returns>Success response</returns>
        /// <response code="200">Status updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("incident/status")]
        public async Task<IActionResult> UpdateIncidentStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (request == null || request.IncidentId <= 0 || string.IsNullOrEmpty(request.Status))
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }
                
                await _trackingService.UpdateIncidentStatusAsync(request.IncidentId, request.Status);
                
                _logger.LogInformation("Updated status for incident {IncidentId}: {Status}", 
                    request.IncidentId, request.Status);
                
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Incident status updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating incident status");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error updating incident status"
                });
            }
        }

        /// <summary>
        /// Update responder location coordinates
        /// </summary>
        /// <param name="request">Responder location update request</param>
        /// <returns>Success response</returns>
        /// <response code="200">Location updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("responder/location")]
        public async Task<IActionResult> UpdateResponderLocation([FromBody] ResponderLocationRequest request)
        {
            try
            {
                if (request == null || request.ResponderId <= 0)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }
                
                await _trackingService.UpdateResponderLocationAsync(
                    request.ResponderId, 
                    request.Latitude, 
                    request.Longitude);
                
                _logger.LogInformation("Updated location for responder {ResponderId}: {Lat}, {Lng}", 
                    request.ResponderId, request.Latitude, request.Longitude);
                
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Responder location updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating responder location");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error updating responder location"
                });
            }
        }

        /// <summary>
        /// Get map statistics
        /// </summary>
        /// <returns>Map statistics</returns>
        /// <response code="200">Statistics retrieved successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetMapStatistics()
        {
            try
            {
                var mapData = await _trackingService.GetMapDataAsync();
                
                return Ok(new ApiResponse<MapStatistics>
                {
                    Success = true,
                    Data = mapData.Statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving map statistics");
                return StatusCode(500, new ApiResponse<MapStatistics>
                {
                    Success = false,
                    Message = "Error retrieving map statistics"
                });
            }
        }

        /// <summary>
        /// Get disaster types for filtering
        /// </summary>
        /// <returns>List of available disaster types</returns>
        /// <response code="200">Disaster types retrieved successfully</response>
        [HttpGet("disaster-types")]
        public IActionResult GetDisasterTypes()
        {
            var disasterTypes = new[]
            {
                new { Value = "medical", Label = "Medical Emergency" },
                new { Value = "fire", Label = "Fire" },
                new { Value = "traffic", Label = "Traffic Accident" },
                new { Value = "crime", Label = "Crime" },
                new { Value = "weather", Label = "Weather Related" },
                new { Value = "hazardous", Label = "Hazardous Material" },
                new { Value = "utility", Label = "Utility Emergency" },
                new { Value = "other", Label = "Other" }
            };
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = disasterTypes
            });
        }

        /// <summary>
        /// Get priority levels for filtering
        /// </summary>
        /// <returns>List of available priority levels</returns>
        /// <response code="200">Priority levels retrieved successfully</response>
        [HttpGet("priority-levels")]
        public IActionResult GetPriorityLevels()
        {
            var priorityLevels = new[]
            {
                new { Value = "emergency", Label = "Emergency", Color = "#e74c3c" },
                new { Value = "high", Label = "High", Color = "#f39c12" },
                new { Value = "medium", Label = "Medium", Color = "#3498db" },
                new { Value = "low", Label = "Low", Color = "#27ae60" }
            };
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = priorityLevels
            });
        }

        /// <summary>
        /// Get status options for filtering
        /// </summary>
        /// <returns>List of available status options</returns>
        /// <response code="200">Status options retrieved successfully</response>
        [HttpGet("status-options")]
        public IActionResult GetStatusOptions()
        {
            var statusOptions = new[]
            {
                new { Value = "ongoing", Label = "Ongoing", Color = "#f39c12" },
                new { Value = "escalated", Label = "Escalated", Color = "#e74c3c" },
                new { Value = "safe", Label = "Safe", Color = "#27ae60" },
                new { Value = "completed", Label = "Completed", Color = "#95a5a6" }
            };
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = statusOptions
            });
        }
    }

    // =============================================================================
    // REQUEST MODELS
    // =============================================================================

    /// <summary>
    /// Location update request model
    /// </summary>
    public class LocationUpdateRequest
    {
        public int IncidentId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    /// <summary>
    /// Status update request model
    /// </summary>
    public class StatusUpdateRequest
    {
        public int IncidentId { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Responder location update request model
    /// </summary>
    public class ResponderLocationRequest
    {
        public int ResponderId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
} 