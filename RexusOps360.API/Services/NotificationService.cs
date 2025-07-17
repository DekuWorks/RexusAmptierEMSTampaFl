using Microsoft.AspNetCore.SignalR;
using RexusOps360.API.Hubs;

namespace RexusOps360.API.Services
{
    public interface INotificationService
    {
        Task SendEmergencyAlertAsync(string message, string priority, string area);
        Task SendNotificationToRoleAsync(string role, string title, string message, string type);
        Task SendIncidentUpdateAsync(object incidentData);
        Task SendEquipmentUpdateAsync(object equipmentData);
        Task SendWeatherAlertAsync(string alert, string severity);
        Task SendSystemHealthUpdateAsync(object healthData);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<EmsHub> _hubContext;

        public NotificationService(IHubContext<EmsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendEmergencyAlertAsync(string message, string priority, string area)
        {
            var alertData = new
            {
                Message = message,
                Priority = priority,
                Area = area,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("EmergencyAlert", alertData);
        }

        public async Task SendNotificationToRoleAsync(string role, string title, string message, string type)
        {
            var notificationData = new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group(role).SendAsync("Notification", notificationData);
        }

        public async Task SendIncidentUpdateAsync(object incidentData)
        {
            await _hubContext.Clients.Group("dispatchers").SendAsync("IncidentUpdated", incidentData);
            await _hubContext.Clients.Group("admin").SendAsync("IncidentUpdated", incidentData);
            await _hubContext.Clients.Group("responders").SendAsync("IncidentUpdated", incidentData);
        }

        public async Task SendEquipmentUpdateAsync(object equipmentData)
        {
            await _hubContext.Clients.Group("dispatchers").SendAsync("EquipmentUpdated", equipmentData);
            await _hubContext.Clients.Group("admin").SendAsync("EquipmentUpdated", equipmentData);
        }

        public async Task SendWeatherAlertAsync(string alert, string severity)
        {
            var weatherData = new
            {
                Alert = alert,
                Severity = severity,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("WeatherAlert", weatherData);
        }

        public async Task SendSystemHealthUpdateAsync(object healthData)
        {
            await _hubContext.Clients.Group("admin").SendAsync("SystemHealthUpdated", healthData);
        }
    }
} 