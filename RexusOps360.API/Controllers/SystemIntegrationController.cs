using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Models;
using RexusOps360.API.Services;
using Microsoft.EntityFrameworkCore;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SystemIntegrationController : ControllerBase
    {
        private readonly ISystemIntegrationService _integrationService;
        private readonly ILogger<SystemIntegrationController> _logger;

        public SystemIntegrationController(
            ISystemIntegrationService integrationService,
            ILogger<SystemIntegrationController> logger)
        {
            _integrationService = integrationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetIntegrations([FromQuery] string? type = null)
        {
            try
            {
                var integrations = await _integrationService.GetActiveIntegrationsAsync(type);
                return Ok(new { integrations = integrations });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system integrations");
                return StatusCode(500, new { error = "Error retrieving system integrations" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetIntegration(int id)
        {
            try
            {
                var integrations = await _integrationService.GetActiveIntegrationsAsync();
                var integration = integrations.FirstOrDefault(i => i.Id == id);
                
                if (integration == null)
                    return NotFound(new { error = "Integration not found" });

                return Ok(new { integration = integration });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving integration {IntegrationId}", id);
                return StatusCode(500, new { error = "Error retrieving integration" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateIntegration([FromBody] SystemIntegration integration)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { error = "Invalid data provided" });

                var createdIntegration = await _integrationService.CreateIntegrationAsync(integration);
                if (createdIntegration == null)
                    return StatusCode(500, new { error = "Error creating integration" });

                return CreatedAtAction(nameof(GetIntegration), new { id = createdIntegration.Id }, new
                {
                    message = "Integration created successfully",
                    integration = createdIntegration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating system integration");
                return StatusCode(500, new { error = "Error creating system integration" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIntegration(int id, [FromBody] SystemIntegration integration)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { error = "Invalid data provided" });

                integration.Id = id;
                var success = await _integrationService.UpdateIntegrationAsync(integration);
                
                if (!success)
                    return NotFound(new { error = "Integration not found" });

                return Ok(new { message = "Integration updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating integration {IntegrationId}", id);
                return StatusCode(500, new { error = "Error updating integration" });
            }
        }

        [HttpPost("{id}/test")]
        public async Task<IActionResult> TestIntegration(int id)
        {
            try
            {
                var success = await _integrationService.TestIntegrationAsync(id);
                
                return Ok(new
                {
                    integration_id = id,
                    test_successful = success,
                    message = success ? "Integration test successful" : "Integration test failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing integration {IntegrationId}", id);
                return StatusCode(500, new { error = "Error testing integration" });
            }
        }

        [HttpGet("{id}/data")]
        public async Task<IActionResult> GetIntegrationData(int id, [FromQuery] DateTime? startTime = null)
        {
            try
            {
                var data = await _integrationService.GetIntegrationDataAsync(id, startTime);
                
                return Ok(new
                {
                    integration_id = id,
                    data = data,
                    count = data.Count,
                    time_range = startTime.HasValue ? $"From {startTime.Value:yyyy-MM-dd HH:mm:ss}" : "All data"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving integration data for {IntegrationId}", id);
                return StatusCode(500, new { error = "Error retrieving integration data" });
            }
        }

        [HttpPost("sync/scada")]
        public async Task<IActionResult> SyncScadaData()
        {
            try
            {
                var success = await _integrationService.SyncScadaDataAsync();
                
                return Ok(new
                {
                    sync_type = "SCADA",
                    success = success,
                    message = success ? "SCADA data sync completed" : "SCADA data sync failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing SCADA data");
                return StatusCode(500, new { error = "Error syncing SCADA data" });
            }
        }

        [HttpPost("sync/gps")]
        public async Task<IActionResult> SyncGpsData()
        {
            try
            {
                var success = await _integrationService.SyncGpsDataAsync();
                
                return Ok(new
                {
                    sync_type = "GPS",
                    success = success,
                    message = success ? "GPS data sync completed" : "GPS data sync failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing GPS data");
                return StatusCode(500, new { error = "Error syncing GPS data" });
            }
        }

        [HttpPost("sync/weather")]
        public async Task<IActionResult> SyncWeatherData()
        {
            try
            {
                var success = await _integrationService.SyncWeatherDataAsync();
                
                return Ok(new
                {
                    sync_type = "Weather",
                    success = success,
                    message = success ? "Weather data sync completed" : "Weather data sync failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing weather data");
                return StatusCode(500, new { error = "Error syncing weather data" });
            }
        }

        [HttpPost("sync/gis")]
        public async Task<IActionResult> SyncGisData()
        {
            try
            {
                var success = await _integrationService.SyncGisDataAsync();
                
                return Ok(new
                {
                    sync_type = "GIS",
                    success = success,
                    message = success ? "GIS data sync completed" : "GIS data sync failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing GIS data");
                return StatusCode(500, new { error = "Error syncing GIS data" });
            }
        }

        [HttpGet("types")]
        public IActionResult GetIntegrationTypes()
        {
            var types = new[]
            {
                new { type = "SCADA", description = "Supervisory Control and Data Acquisition systems" },
                new { type = "GPS", description = "Global Positioning System for vehicle/asset tracking" },
                new { type = "Weather", description = "Weather service APIs (NOAA, OpenWeatherMap, etc.)" },
                new { type = "GIS", description = "Geographic Information Systems (Esri, PostGIS, etc.)" },
                new { type = "CMMS", description = "Computerized Maintenance Management Systems" },
                new { type = "CustomerInfo", description = "Customer information systems" },
                new { type = "WorkManagement", description = "Work order and management systems" },
                new { type = "AssetManagement", description = "Asset management and tracking systems" }
            };

            return Ok(new { integration_types = types });
        }

        [HttpGet("capabilities")]
        public IActionResult GetIntegrationCapabilities()
        {
            var capabilities = new
            {
                scada = new
                {
                    protocols = new[] { "REST", "MQTT", "OPC UA", "Modbus" },
                    data_types = new[] { "Pressure", "Flow", "Temperature", "Level", "Status" },
                    features = new[] { "Real-time monitoring", "Alarm management", "Historical data" }
                },
                gps = new
                {
                    protocols = new[] { "REST", "WebSocket", "MQTT" },
                    data_types = new[] { "Location", "Speed", "Heading", "Status" },
                    features = new[] { "Real-time tracking", "Route optimization", "Geofencing" }
                },
                weather = new
                {
                    protocols = new[] { "REST", "GraphQL" },
                    data_types = new[] { "Temperature", "Precipitation", "Wind", "Humidity", "Pressure" },
                    features = new[] { "Forecast data", "Historical data", "Severe weather alerts" }
                },
                gis = new
                {
                    protocols = new[] { "REST", "WMS", "WFS", "GeoJSON" },
                    data_types = new[] { "Asset locations", "Infrastructure data", "Spatial analysis" },
                    features = new[] { "Spatial queries", "Map visualization", "Asset tracking" }
                }
            };

            return Ok(new { capabilities = capabilities });
        }
    }
} 