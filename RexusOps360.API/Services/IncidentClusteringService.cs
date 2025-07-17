using RexusOps360.API.Models;
using RexusOps360.API.Data;
using Microsoft.EntityFrameworkCore;

namespace RexusOps360.API.Services
{
    public interface IIncidentClusteringService
    {
        Task<string?> ClusterIncidentAsync(Incident incident);
        Task<List<Incident>> GetClusteredIncidentsAsync(string clusterId);
        Task<List<object>> GetIncidentClustersAsync(string? utilityType = null);
        Task<bool> IsSimilarIncidentAsync(Incident newIncident, Incident existingIncident);
        Task<List<Incident>> GetSimilarIncidentsAsync(Incident incident, double radiusKm = 1.0, int timeWindowMinutes = 30);
    }

    public class IncidentClusteringService : IIncidentClusteringService
    {
        private readonly EmsDbContext _context;
        private readonly ILogger<IncidentClusteringService> _logger;

        public IncidentClusteringService(EmsDbContext context, ILogger<IncidentClusteringService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string?> ClusterIncidentAsync(Incident incident)
        {
            try
            {
                // Find similar incidents within time and location window
                var similarIncidents = await GetSimilarIncidentsAsync(incident);
                
                if (similarIncidents.Any())
                {
                    // Use the first incident's cluster ID or create a new one
                    var clusterId = similarIncidents.First().ClusterId ?? $"CLUSTER_{DateTime.UtcNow:yyyyMMddHHmmss}";
                    
                    // Update all similar incidents with the same cluster ID
                    foreach (var similar in similarIncidents)
                    {
                        similar.ClusterId = clusterId;
                        similar.UpdatedAt = DateTime.UtcNow;
                    }
                    
                    // Set cluster ID for the new incident
                    incident.ClusterId = clusterId;
                    
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Clustered incident {incident.Id} with cluster {clusterId}");
                    return clusterId;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clustering incident {IncidentId}", incident.Id);
                return null;
            }
        }

        public async Task<List<Incident>> GetClusteredIncidentsAsync(string clusterId)
        {
            return await _context.Incidents
                .Where(i => i.ClusterId == clusterId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<object>> GetIncidentClustersAsync(string? utilityType = null)
        {
            var query = _context.Incidents
                .Where(i => !string.IsNullOrEmpty(i.ClusterId))
                .GroupBy(i => i.ClusterId);

            if (!string.IsNullOrEmpty(utilityType))
            {
                query = query.Where(g => g.Any(i => i.UtilityType == utilityType));
            }

            var clusters = await query
                .Select(g => new
                {
                    ClusterId = g.Key,
                    IncidentCount = g.Count(),
                    FirstIncident = g.OrderBy(i => i.CreatedAt).First(),
                    LastIncident = g.OrderByDescending(i => i.CreatedAt).First(),
                    UtilityType = g.First().UtilityType,
                    Category = g.First().Category,
                    Zone = g.First().Zone,
                    Priority = g.Max(i => i.Priority == "High" ? 3 : i.Priority == "Medium" ? 2 : 1),
                    SeverityLevel = g.Max(i => i.SeverityLevel ?? 0),
                    TotalContacts = g.Count(i => !string.IsNullOrEmpty(i.ContactInfo)),
                    TotalRemarks = g.Count(i => !string.IsNullOrEmpty(i.Remarks))
                })
                .ToListAsync();

            return clusters.Cast<object>().ToList();
        }

        public async Task<bool> IsSimilarIncidentAsync(Incident newIncident, Incident existingIncident)
        {
            // Check if incidents are similar based on multiple criteria
            var timeDiff = Math.Abs((newIncident.CreatedAt - existingIncident.CreatedAt).TotalMinutes);
            var locationDiff = CalculateDistance(
                newIncident.Latitude ?? 0, newIncident.Longitude ?? 0,
                existingIncident.Latitude ?? 0, existingIncident.Longitude ?? 0
            );

            // Similar if:
            // 1. Same utility type and category
            // 2. Within 1km distance
            // 3. Within 30 minutes time window
            // 4. Same zone (if specified)
            return newIncident.UtilityType == existingIncident.UtilityType &&
                   newIncident.Category == existingIncident.Category &&
                   locationDiff <= 1.0 && // 1km radius
                   timeDiff <= 30 && // 30 minutes
                   (string.IsNullOrEmpty(newIncident.Zone) || 
                    string.IsNullOrEmpty(existingIncident.Zone) || 
                    newIncident.Zone == existingIncident.Zone);
        }

        public async Task<List<Incident>> GetSimilarIncidentsAsync(Incident incident, double radiusKm = 1.0, int timeWindowMinutes = 30)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-timeWindowMinutes);
            
            var similarIncidents = await _context.Incidents
                .Where(i => i.CreatedAt >= cutoffTime &&
                           i.UtilityType == incident.UtilityType &&
                           i.Category == incident.Category &&
                           i.Status == "Active")
                .ToListAsync();

            // Filter by distance
            var nearbyIncidents = new List<Incident>();
            foreach (var existing in similarIncidents)
            {
                if (incident.Latitude.HasValue && incident.Longitude.HasValue &&
                    existing.Latitude.HasValue && existing.Longitude.HasValue)
                {
                    var distance = CalculateDistance(
                        incident.Latitude.Value, incident.Longitude.Value,
                        existing.Latitude.Value, existing.Longitude.Value
                    );
                    
                    if (distance <= radiusKm)
                    {
                        nearbyIncidents.Add(existing);
                    }
                }
            }

            return nearbyIncidents;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
} 