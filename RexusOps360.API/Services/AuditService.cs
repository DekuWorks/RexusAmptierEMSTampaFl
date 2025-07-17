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
                EntityType = "UserAction",
                EntityId = userId,
                Severity = "Info",
                Timestamp = DateTime.UtcNow,
                IsSuccessful = true
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
                EntityType = "SystemEvent",
                EntityId = "SYSTEM",
                Severity = severity,
                Timestamp = DateTime.UtcNow,
                IsSuccessful = true
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
                EntityType = "SecurityEvent",
                EntityId = userId,
                Severity = "Error",
                Timestamp = DateTime.UtcNow,
                IsSuccessful = false
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
            var query = _context.AuditLogs.Where(a => a.EntityType == "SecurityEvent");

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
        }
    }
} 