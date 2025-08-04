/*
 * RexusOps360 EMS API - Real-Time Tracking Service
 * 
 * This service handles real-time incident tracking, location updates,
 * and status monitoring for the EMS system.
 * 
 * Features:
 * - Real-time location tracking
 * - Incident status updates
 * - GPS coordinate management
 * - Responder location tracking
 * - Live map data generation
 * 
 * Author: RexusOps360 Development Team
 * Version: 1.0.0
 * Last Updated: 2025-01-17
 */

using RexusOps360.API.Models;
using RexusOps360.API.Hubs;
using RexusOps360.API.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace RexusOps360.API.Services
{
    /// <summary>
    /// Real-time tracking service for incident and responder locations
    /// </summary>
    public interface IRealTimeTrackingService
    {
        Task UpdateIncidentLocationAsync(int incidentId, double latitude, double longitude);
        Task UpdateIncidentStatusAsync(int incidentId, string status);
        Task UpdateResponderLocationAsync(int responderId, double latitude, double longitude);
        Task<List<Incident>> GetActiveIncidentsAsync();
        Task<List<Responder>> GetActiveRespondersAsync();
        Task<MapData> GetMapDataAsync();
    }

    public class RealTimeTrackingService : IRealTimeTrackingService
    {
        private readonly ILogger<RealTimeTrackingService> _logger;
        private readonly IHubContext<EmsHub> _hubContext;
        private readonly EmsDbContext _context;

        public RealTimeTrackingService(
            ILogger<RealTimeTrackingService> logger,
            IHubContext<EmsHub> hubContext,
            EmsDbContext context)
        {
            _logger = logger;
            _hubContext = hubContext;
            _context = context;
        }

        /// <summary>
        /// Update incident location coordinates
        /// </summary>
        public async Task UpdateIncidentLocationAsync(int incidentId, double latitude, double longitude)
        {
            try
            {
                var incident = await _context.Incidents.FindAsync(incidentId);
                if (incident != null)
                {
                    incident.Latitude = latitude;
                    incident.Longitude = longitude;
                    incident.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // Notify connected clients
                    await _hubContext.Clients.All.SendAsync("IncidentLocationUpdated", new
                    {
                        IncidentId = incidentId,
                        Latitude = latitude,
                        Longitude = longitude,
                        UpdatedAt = DateTime.UtcNow
                    });

                    _logger.LogInformation("Updated location for incident {IncidentId}: {Lat}, {Lng}", 
                        incidentId, latitude, longitude);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating incident location for incident {IncidentId}", incidentId);
                throw;
            }
        }

        /// <summary>
        /// Update incident status
        /// </summary>
        public async Task UpdateIncidentStatusAsync(int incidentId, string status)
        {
            try
            {
                var incident = await _context.Incidents.FindAsync(incidentId);
                if (incident != null)
                {
                    incident.Status = status;
                    incident.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // Notify connected clients
                    await _hubContext.Clients.All.SendAsync("IncidentStatusUpdated", new
                    {
                        IncidentId = incidentId,
                        Status = status,
                        UpdatedAt = DateTime.UtcNow
                    });

                    _logger.LogInformation("Updated status for incident {IncidentId}: {Status}", 
                        incidentId, status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating incident status for incident {IncidentId}", incidentId);
                throw;
            }
        }

        /// <summary>
        /// Update responder location coordinates
        /// </summary>
        public async Task UpdateResponderLocationAsync(int responderId, double latitude, double longitude)
        {
            try
            {
                var responder = await _context.Responders.FindAsync(responderId);
                if (responder != null)
                {
                    responder.Latitude = latitude;
                    responder.Longitude = longitude;
                    responder.LastLocationUpdate = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // Notify connected clients
                    await _hubContext.Clients.All.SendAsync("ResponderLocationUpdated", new
                    {
                        ResponderId = responderId,
                        Latitude = latitude,
                        Longitude = longitude,
                        UpdatedAt = DateTime.UtcNow
                    });

                    _logger.LogInformation("Updated location for responder {ResponderId}: {Lat}, {Lng}", 
                        responderId, latitude, longitude);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating responder location for responder {ResponderId}", responderId);
                throw;
            }
        }

        /// <summary>
        /// Get all active incidents
        /// </summary>
        public async Task<List<Incident>> GetActiveIncidentsAsync()
        {
            try
            {
                return await _context.Incidents
                    .Where(i => i.Status != "completed" && i.Status != "cancelled")
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active incidents");
                throw;
            }
        }

        /// <summary>
        /// Get all active responders
        /// </summary>
        public async Task<List<Responder>> GetActiveRespondersAsync()
        {
            try
            {
                return await _context.Responders
                    .Where(r => r.Status == "active" || r.Status == "responding")
                    .OrderBy(r => r.LastLocationUpdate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active responders");
                throw;
            }
        }

        /// <summary>
        /// Get map data for real-time display
        /// </summary>
        public async Task<MapData> GetMapDataAsync()
        {
            try
            {
                var incidents = await GetActiveIncidentsAsync();
                var responders = await GetActiveRespondersAsync();

                return new MapData
                {
                    Incidents = incidents.Select(i => new IncidentMapItem
                    {
                        Id = i.Id,
                        Type = i.Type,
                        Priority = i.Priority,
                        Status = i.Status,
                        Title = i.Description,
                        Location = i.Location,
                        Latitude = i.Latitude ?? 0.0,
                        Longitude = i.Longitude ?? 0.0,
                        ReportedAt = i.CreatedAt,
                        Responders = 0, // TODO: Implement responder count logic
                        Eta = CalculateEta(i)
                    }).ToList(),
                    Responders = responders.Select(r => new ResponderMapItem
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Role = r.Role,
                        Status = r.Status,
                        Latitude = r.Latitude ?? 0.0,
                        Longitude = r.Longitude ?? 0.0,
                        LastUpdate = r.LastLocationUpdate
                    }).ToList(),
                    Statistics = new MapStatistics
                    {
                        TotalIncidents = incidents.Count,
                        EmergencyIncidents = incidents.Count(i => i.Priority == "emergency"),
                        OngoingIncidents = incidents.Count(i => i.Status == "ongoing"),
                        EscalatedIncidents = incidents.Count(i => i.Status == "escalated"),
                        SafeIncidents = incidents.Count(i => i.Status == "safe"),
                        ActiveResponders = responders.Count
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving map data");
                throw;
            }
        }

        /// <summary>
        /// Calculate estimated time of arrival for an incident
        /// </summary>
        private string CalculateEta(Incident incident)
        {
            // Simple ETA calculation based on priority and status
            var baseMinutes = incident.Priority switch
            {
                "emergency" => 2,
                "high" => 5,
                "medium" => 10,
                "low" => 15,
                _ => 8
            };

            var statusMultiplier = incident.Status switch
            {
                "ongoing" => 1.0,
                "escalated" => 0.5,
                "safe" => 2.0,
                _ => 1.0
            };

            var etaMinutes = Math.Max(1, (int)(baseMinutes * statusMultiplier));
            return $"{etaMinutes} minutes";
        }
    }

    // =============================================================================
    // MAP DATA MODELS
    // =============================================================================

    /// <summary>
    /// Complete map data for real-time display
    /// </summary>
    public class MapData
    {
        public List<IncidentMapItem> Incidents { get; set; } = new List<IncidentMapItem>();
        public List<ResponderMapItem> Responders { get; set; } = new List<ResponderMapItem>();
        public MapStatistics Statistics { get; set; } = new MapStatistics();
    }

    /// <summary>
    /// Incident data for map display
    /// </summary>
    public class IncidentMapItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime ReportedAt { get; set; }
        public int Responders { get; set; }
        public string Eta { get; set; } = string.Empty;
    }

    /// <summary>
    /// Responder data for map display
    /// </summary>
    public class ResponderMapItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? LastUpdate { get; set; }
    }

    /// <summary>
    /// Map statistics for dashboard display
    /// </summary>
    public class MapStatistics
    {
        public int TotalIncidents { get; set; }
        public int EmergencyIncidents { get; set; }
        public int OngoingIncidents { get; set; }
        public int EscalatedIncidents { get; set; }
        public int SafeIncidents { get; set; }
        public int ActiveResponders { get; set; }
    }
} 