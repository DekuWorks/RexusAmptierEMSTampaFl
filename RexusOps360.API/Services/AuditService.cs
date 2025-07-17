using Microsoft.EntityFrameworkCore;
using RexusOps360.API.Data;
using RexusOps360.API.Models;

namespace RexusOps360.API.Services
{
    public interface IAuditService
    {
        Task LogUserActionAsync(string userId, string action, string details, string ipAddress);
        Task LogSystemEventAsync(string eventType, string details, string severity);
        Task LogSecurityEventAsync(string eventType, string userId, string details, string ipAddress);
        Task<List<AuditLog>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? userId = null);
        Task<List<AuditLog>> GetSecurityEventsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // "UserAction", "SystemEvent", "SecurityEvent"
        public string Severity { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class AuditService : IAuditService
    {
        private readonly EmsDbContext _context;

        public AuditService(EmsDbContext context)
        {
            _context = context;
        }

        public async Task LogUserActionAsync(string userId, string action, string details, string ipAddress)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                IpAddress = ipAddress,
                EventType = "UserAction",
                Severity = "Low",
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogSystemEventAsync(string eventType, string details, string severity)
        {
            var auditLog = new AuditLog
            {
                UserId = "SYSTEM",
                Action = eventType,
                Details = details,
                IpAddress = "SYSTEM",
                EventType = "SystemEvent",
                Severity = severity,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogSecurityEventAsync(string eventType, string userId, string details, string ipAddress)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = eventType,
                Details = details,
                IpAddress = ipAddress,
                EventType = "SecurityEvent",
                Severity = "High",
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? userId = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(a => a.UserId == userId);

            return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
        }

        public async Task<List<AuditLog>> GetSecurityEventsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AuditLogs.Where(a => a.EventType == "SecurityEvent");

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
        }
    }
} 