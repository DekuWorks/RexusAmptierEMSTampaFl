using RexusOps360.API.Models;
using RexusOps360.API.Data;
using Microsoft.EntityFrameworkCore;

namespace RexusOps360.API.Services
{
    public interface IHotspotDetectionService
    {
        Task<List<Hotspot>> DetectHotspotsAsync(string? utilityType = null);
        Task<Hotspot?> CreateHotspotAsync(Incident incident, List<Incident> relatedIncidents);
        Task<List<HotspotAlert>> GenerateHotspotAlertsAsync(Hotspot hotspot);
        Task<bool> UpdateHotspotStatusAsync(int hotspotId, string status);
        Task<List<Hotspot>> GetActiveHotspotsAsync(string? utilityType = null);
        Task<List<object>> GetHotspotAnalyticsAsync();
    }

    public class HotspotDetectionService : IHotspotDetectionService
    {
        private readonly EmsDbContext _context;
        private readonly ILogger<HotspotDetectionService> _logger;
        private readonly INotificationService _notificationService;

        public HotspotDetectionService(
            EmsDbContext context, 
            ILogger<HotspotDetectionService> logger,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<List<Hotspot>> DetectHotspotsAsync(string? utilityType = null)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddHours(-2); // Look back 2 hours
                
                var query = _context.Incidents
                    .Where(i => i.CreatedAt >= cutoffTime && i.Status == "Active");

                if (!string.IsNullOrEmpty(utilityType))
                {
                    query = query.Where(i => i.UtilityType == utilityType);
                }

                var recentIncidents = await query.ToListAsync();
                var hotspots = new List<Hotspot>();

                // Group incidents by location and type
                var incidentGroups = recentIncidents
                    .GroupBy(i => new { i.UtilityType, i.Category, i.Zone })
                    .Where(g => g.Count() >= 3); // Minimum 3 incidents to form a hotspot

                foreach (var group in incidentGroups)
                {
                    var incidents = group.ToList();
                    var firstIncident = incidents.OrderBy(i => i.CreatedAt).First();
                    
                    // Calculate hotspot center and severity
                    var avgLat = incidents.Average(i => i.Latitude ?? 0);
                    var avgLon = incidents.Average(i => i.Longitude ?? 0);
                    var maxSeverity = incidents.Max(i => i.SeverityLevel ?? 0);
                    var highPriorityCount = incidents.Count(i => i.Priority == "High");
                    
                    var severity = DetermineSeverity(incidents.Count, maxSeverity, highPriorityCount);
                    
                    var hotspot = new Hotspot
                    {
                        Name = $"Hotspot - {firstIncident.Category} in {firstIncident.Zone ?? "Unknown Zone"}",
                        Type = "Geographic",
                        UtilityType = firstIncident.UtilityType ?? "Combined",
                        Location = firstIncident.Location,
                        Latitude = avgLat,
                        Longitude = avgLon,
                        RadiusMeters = 500, // 500m radius
                        Severity = severity,
                        Description = $"Multiple {firstIncident.Category} incidents detected in {firstIncident.Zone ?? "this area"}",
                        IncidentCount = incidents.Count,
                        Threshold = 3,
                        TimeWindowMinutes = 120,
                        Status = "Active",
                        FirstDetected = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    };

                    hotspots.Add(hotspot);
                }

                // Save new hotspots
                foreach (var hotspot in hotspots)
                {
                    var existingHotspot = await _context.Hotspots
                        .FirstOrDefaultAsync(h => h.Location == hotspot.Location && 
                                                h.UtilityType == hotspot.UtilityType &&
                                                h.Status == "Active");

                    if (existingHotspot == null)
                    {
                        _context.Hotspots.Add(hotspot);
                        await GenerateHotspotAlertsAsync(hotspot);
                    }
                    else
                    {
                        // Update existing hotspot
                        existingHotspot.IncidentCount = hotspot.IncidentCount;
                        existingHotspot.LastUpdated = DateTime.UtcNow;
                        existingHotspot.Severity = hotspot.Severity;
                    }
                }

                await _context.SaveChangesAsync();
                return hotspots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting hotspots");
                return new List<Hotspot>();
            }
        }

        public async Task<Hotspot?> CreateHotspotAsync(Incident incident, List<Incident> relatedIncidents)
        {
            try
            {
                var hotspot = new Hotspot
                {
                    Name = $"Hotspot - {incident.Category} in {incident.Zone ?? "Unknown Zone"}",
                    Type = "Geographic",
                    UtilityType = incident.UtilityType ?? "Combined",
                    Location = incident.Location,
                    Latitude = incident.Latitude,
                    Longitude = incident.Longitude,
                    RadiusMeters = 500,
                    Severity = DetermineSeverity(relatedIncidents.Count, incident.SeverityLevel ?? 0, 
                                               relatedIncidents.Count(i => i.Priority == "High")),
                    Description = $"Multiple {incident.Category} incidents detected in {incident.Zone ?? "this area"}",
                    IncidentCount = relatedIncidents.Count + 1,
                    Threshold = 3,
                    TimeWindowMinutes = 120,
                    Status = "Active",
                    FirstDetected = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                _context.Hotspots.Add(hotspot);
                await _context.SaveChangesAsync();

                await GenerateHotspotAlertsAsync(hotspot);
                return hotspot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hotspot for incident {IncidentId}", incident.Id);
                return null;
            }
        }

        public async Task<List<HotspotAlert>> GenerateHotspotAlertsAsync(Hotspot hotspot)
        {
            var alerts = new List<HotspotAlert>();

            try
            {
                var alertLevel = hotspot.Severity switch
                {
                    "Critical" => "Critical",
                    "High" => "Warning",
                    _ => "Info"
                };

                var alert = new HotspotAlert
                {
                    HotspotId = hotspot.Id,
                    Title = $"Hotspot Detected: {hotspot.Name}",
                    Message = $"A {hotspot.Severity.ToLower()} priority hotspot has been detected in {hotspot.Location}. " +
                             $"This area has experienced {hotspot.IncidentCount} incidents in the last {hotspot.TimeWindowMinutes} minutes. " +
                             $"Immediate attention is required.",
                    AlertLevel = alertLevel,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };

                alerts.Add(alert);
                _context.HotspotAlerts.Add(alert);

                // Send real-time notification
                await _notificationService.SendEmergencyAlertAsync(
                    alert.Message,
                    alertLevel,
                    hotspot.Location ?? "Unknown Area"
                );

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating hotspot alerts for hotspot {HotspotId}", hotspot.Id);
            }

            return alerts;
        }

        public async Task<bool> UpdateHotspotStatusAsync(int hotspotId, string status)
        {
            try
            {
                var hotspot = await _context.Hotspots.FindAsync(hotspotId);
                if (hotspot == null) return false;

                hotspot.Status = status;
                hotspot.LastUpdated = DateTime.UtcNow;

                if (status == "Resolved")
                {
                    hotspot.ResolvedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotspot status for hotspot {HotspotId}", hotspotId);
                return false;
            }
        }

        public async Task<List<Hotspot>> GetActiveHotspotsAsync(string? utilityType = null)
        {
            var query = _context.Hotspots.Where(h => h.Status == "Active");

            if (!string.IsNullOrEmpty(utilityType))
            {
                query = query.Where(h => h.UtilityType == utilityType);
            }

            return await query.OrderByDescending(h => h.Severity).ToListAsync();
        }

        public async Task<List<object>> GetHotspotAnalyticsAsync()
        {
            var analytics = new List<object>();

            try
            {
                // Hotspots by utility type
                var byUtility = await _context.Hotspots
                    .GroupBy(h => h.UtilityType)
                    .Select(g => new
                    {
                        UtilityType = g.Key,
                        TotalHotspots = g.Count(),
                        ActiveHotspots = g.Count(h => h.Status == "Active"),
                        CriticalHotspots = g.Count(h => h.Severity == "Critical"),
                        AverageIncidentCount = g.Average(h => h.IncidentCount)
                    })
                    .ToListAsync();

                // Hotspots by severity
                var bySeverity = await _context.Hotspots
                    .GroupBy(h => h.Severity)
                    .Select(g => new
                    {
                        Severity = g.Key,
                        Count = g.Count(),
                        AverageDuration = g.Average(h => h.Duration.TotalMinutes)
                    })
                    .ToListAsync();

                analytics.AddRange(byUtility.Cast<object>());
                analytics.AddRange(bySeverity.Cast<object>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotspot analytics");
            }

            return analytics;
        }

        private string DetermineSeverity(int incidentCount, int maxSeverity, int highPriorityCount)
        {
            if (incidentCount >= 10 || maxSeverity >= 5 || highPriorityCount >= 3)
                return "Critical";
            else if (incidentCount >= 5 || maxSeverity >= 4 || highPriorityCount >= 2)
                return "High";
            else if (incidentCount >= 3 || maxSeverity >= 3)
                return "Medium";
            else
                return "Low";
        }
    }
} 