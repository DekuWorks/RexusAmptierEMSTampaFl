using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Services;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RealTimeController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IGpsTrackingService _gpsTrackingService;

        public RealTimeController(INotificationService notificationService, IGpsTrackingService gpsTrackingService)
        {
            _notificationService = notificationService;
            _gpsTrackingService = gpsTrackingService;
        }

        [HttpPost("notifications/emergency")]
        public async Task<IActionResult> SendEmergencyAlert([FromBody] EmergencyAlertRequest request)
        {
            await _notificationService.SendEmergencyAlertAsync(request.Message, request.Priority, request.Area);
            return Ok(new { message = "Emergency alert sent successfully" });
        }

        [HttpPost("notifications/role")]
        public async Task<IActionResult> SendNotificationToRole([FromBody] RoleNotificationRequest request)
        {
            await _notificationService.SendNotificationToRoleAsync(request.Role, request.Title, request.Message, request.Type);
            return Ok(new { message = "Notification sent successfully" });
        }

        [HttpPost("gps/update-location")]
        public async Task<IActionResult> UpdateResponderLocation([FromBody] GpsUpdateLocationRequest request)
        {
            await _gpsTrackingService.UpdateResponderLocationAsync(request.ResponderId, request.Location, request.Latitude, request.Longitude);
            return Ok(new { message = "Location updated successfully" });
        }

        [HttpGet("gps/responder-locations")]
        public async Task<IActionResult> GetResponderLocations()
        {
            var locations = await _gpsTrackingService.GetResponderLocationsAsync();
            return Ok(locations);
        }

        [HttpGet("gps/responder/{responderId}")]
        public async Task<IActionResult> GetResponderLocation(string responderId)
        {
            var location = await _gpsTrackingService.GetResponderLocationAsync(responderId);
            if (location == null)
                return NotFound(new { error = "Responder location not found" });

            return Ok(location);
        }

        [HttpGet("gps/nearby-responders")]
        public async Task<IActionResult> GetNearbyResponders([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radius = 10)
        {
            var nearbyResponders = await _gpsTrackingService.GetNearbyRespondersAsync(latitude, longitude, radius);
            return Ok(nearbyResponders);
        }

        [HttpPost("weather/alert")]
        public async Task<IActionResult> SendWeatherAlert([FromBody] WeatherAlertRequest request)
        {
            await _notificationService.SendWeatherAlertAsync(request.Alert, request.Severity);
            return Ok(new { message = "Weather alert sent successfully" });
        }

        [HttpPost("system/health")]
        public async Task<IActionResult> SendSystemHealthUpdate([FromBody] object healthData)
        {
            await _notificationService.SendSystemHealthUpdateAsync(healthData);
            return Ok(new { message = "System health update sent successfully" });
        }
    }

    public class EmergencyAlertRequest
    {
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
    }

    public class RoleNotificationRequest
    {
        public string Role { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class GpsUpdateLocationRequest
    {
        public string ResponderId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class WeatherAlertRequest
    {
        public string Alert { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }
} 