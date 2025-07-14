using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Data;
using RexusOps360.API.Models;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private static readonly List<Notification> _notifications = new();
        private static int _nextNotificationId = 1;

        [HttpGet]
        public IActionResult GetNotifications([FromQuery] string? category = null, [FromQuery] int limit = 50)
        {
            var notifications = _notifications
                .Where(n => category == null || n.Category == category)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToList();

            return Ok(new
            {
                notifications = notifications,
                total_count = _notifications.Count,
                unread_count = _notifications.Count(n => !n.IsRead),
                last_updated = DateTime.UtcNow
            });
        }

        [HttpPost]
        public IActionResult CreateNotification([FromBody] CreateNotificationRequest request)
        {
            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest(new { error = "Title and message are required" });
            }

            var notification = new Notification
            {
                Id = _nextNotificationId++,
                Title = request.Title,
                Message = request.Message,
                Category = request.Category ?? "general",
                Priority = request.Priority ?? "normal",
                TargetArea = request.TargetArea ?? "all",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _notifications.Add(notification);

            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }

        [HttpGet("{id}")]
        public IActionResult GetNotification(int id)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == id);
            if (notification == null)
            {
                return NotFound(new { error = "Notification not found" });
            }

            return Ok(notification);
        }

        [HttpPut("{id}/read")]
        public IActionResult MarkAsRead(int id)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == id);
            if (notification == null)
            {
                return NotFound(new { error = "Notification not found" });
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            return Ok(notification);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteNotification(int id)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == id);
            if (notification == null)
            {
                return NotFound(new { error = "Notification not found" });
            }

            _notifications.Remove(notification);
            return NoContent();
        }

        [HttpGet("alerts")]
        public IActionResult GetActiveAlerts()
        {
            var alerts = _notifications
                .Where(n => n.Category == "alert" && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return Ok(new
            {
                active_alerts = alerts,
                alert_count = alerts.Count,
                last_updated = DateTime.UtcNow
            });
        }

        [HttpPost("incident-alert")]
        public IActionResult CreateIncidentAlert([FromBody] CreateIncidentAlertRequest request)
        {
            var incidents = InMemoryStore.GetAllIncidents();
            var incident = incidents.FirstOrDefault(i => i.Id == request.IncidentId);
            
            if (incident == null)
            {
                return NotFound(new { error = "Incident not found" });
            }

            var notification = new Notification
            {
                Id = _nextNotificationId++,
                Title = $"Incident Alert: {incident.Type}",
                Message = $"New {incident.Priority} priority incident at {incident.Location}. {incident.Description}",
                Category = "incident",
                Priority = incident.Priority == "high" ? "high" : "normal",
                TargetArea = "all",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                RelatedIncidentId = incident.Id
            };

            _notifications.Add(notification);

            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }

        [HttpPost("weather-alert")]
        public IActionResult CreateWeatherAlert([FromBody] CreateWeatherAlertRequest request)
        {
            var notification = new Notification
            {
                Id = _nextNotificationId++,
                Title = "Weather Alert",
                Message = request.Message,
                Category = "weather",
                Priority = request.Priority ?? "normal",
                TargetArea = request.TargetArea ?? "all",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _notifications.Add(notification);

            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }

        [HttpPost("system-alert")]
        public IActionResult CreateSystemAlert([FromBody] CreateSystemAlertRequest request)
        {
            var notification = new Notification
            {
                Id = _nextNotificationId++,
                Title = "System Alert",
                Message = request.Message,
                Category = "system",
                Priority = request.Priority ?? "normal",
                TargetArea = "all",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _notifications.Add(notification);

            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }

        [HttpGet("stats")]
        public IActionResult GetNotificationStats()
        {
            var totalNotifications = _notifications.Count;
            var unreadNotifications = _notifications.Count(n => !n.IsRead);
            var todayNotifications = _notifications.Count(n => n.CreatedAt.Date == DateTime.UtcNow.Date);

            var categoryStats = _notifications
                .GroupBy(n => n.Category)
                .Select(g => new
                {
                    category = g.Key,
                    count = g.Count(),
                    unread_count = g.Count(n => !n.IsRead)
                })
                .ToList();

            return Ok(new
            {
                total_notifications = totalNotifications,
                unread_notifications = unreadNotifications,
                today_notifications = todayNotifications,
                category_stats = categoryStats,
                last_updated = DateTime.UtcNow
            });
        }
    }

    public class CreateNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Priority { get; set; }
        public string? TargetArea { get; set; }
    }

    public class CreateIncidentAlertRequest
    {
        public int IncidentId { get; set; }
    }

    public class CreateWeatherAlertRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? Priority { get; set; }
        public string? TargetArea { get; set; }
    }

    public class CreateSystemAlertRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? Priority { get; set; }
    }

    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = "general";
        public string Priority { get; set; } = "normal";
        public string TargetArea { get; set; } = "all";
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public int? RelatedIncidentId { get; set; }
    }
} 