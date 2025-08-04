using Microsoft.AspNetCore.SignalR;
using RexusOps360.API.Hubs;
using RexusOps360.API.Models;
using RexusOps360.API.Data;

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
        Task<Notification> CreateNotificationAsync(string title, string message, string? type = null, string? area = null, int? userId = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<EmsHub> _hubContext;
        private readonly EmsDbContext _context;

        public NotificationService(IHubContext<EmsHub> hubContext, EmsDbContext context)
        {
            _hubContext = hubContext;
            _context = context;
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

        public async Task<Notification> CreateNotificationAsync(string title, string message, string? type = null, string? area = null, int? userId = null)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type ?? "Info",
                Area = area,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }
    }
} 