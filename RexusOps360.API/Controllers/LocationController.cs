using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Data;
using RexusOps360.API.Models;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private static readonly List<LocationUpdate> _locationUpdates = new();
        private static readonly List<GeoFence> _geoFences = new();
        private static int _nextLocationUpdateId = 1;
        private static int _nextGeoFenceId = 1;

        [HttpGet("responders")]
        public IActionResult GetResponderLocations()
        {
            var responders = InMemoryStore.GetAllResponders();
            var latestLocations = new List<object>();

            foreach (var responder in responders)
            {
                var lastUpdate = _locationUpdates
                    .Where(l => l.ResponderId == responder.Id)
                    .OrderByDescending(l => l.Timestamp)
                    .FirstOrDefault();

                latestLocations.Add(new
                {
                    responder_id = responder.Id,
                    responder_name = responder.Name,
                    role = responder.Role,
                    current_location = responder.CurrentLocation,
                    coordinates = lastUpdate?.Coordinates ?? GetDefaultCoordinates(responder.CurrentLocation),
                    last_updated = lastUpdate?.Timestamp ?? DateTime.UtcNow.AddHours(-1),
                    status = responder.Status,
                    is_online = IsResponderOnline(responder.Id)
                });
            }

            return Ok(new
            {
                responder_locations = latestLocations,
                total_responders = responders.Count,
                online_responders = latestLocations.Count(l => l.GetType().GetProperty("is_online")?.GetValue(l) is bool isOnline && isOnline),
                last_updated = DateTime.UtcNow
            });
        }

        [HttpPost("responders/{responderId}/location")]
        public IActionResult UpdateResponderLocation(int responderId, [FromBody] UpdateLocationRequest request)
        {
            var responder = InMemoryStore.GetAllResponders().FirstOrDefault(r => r.Id == responderId);
            if (responder == null)
            {
                return NotFound(new { error = "Responder not found" });
            }

            var locationUpdate = new LocationUpdate
            {
                Id = _nextLocationUpdateId++,
                ResponderId = responderId,
                Coordinates = request.Coordinates,
                Address = request.Address ?? "Unknown",
                Timestamp = DateTime.UtcNow,
                Speed = request.Speed,
                Heading = request.Heading,
                Accuracy = request.Accuracy
            };

            _locationUpdates.Add(locationUpdate);

            // Update responder's current location
            responder.CurrentLocation = request.Address ?? "Unknown";

            // Check for geo-fence alerts
            var geoFenceAlerts = CheckGeoFenceAlerts(responderId, request.Coordinates);

            return Ok(new
            {
                location_update = locationUpdate,
                responder_updated = true,
                geo_fence_alerts = geoFenceAlerts,
                last_updated = DateTime.UtcNow
            });
        }

        [HttpGet("responders/{responderId}/tracking")]
        public IActionResult GetResponderTracking(int responderId, [FromQuery] int hours = 24)
        {
            var responder = InMemoryStore.GetAllResponders().FirstOrDefault(r => r.Id == responderId);
            if (responder == null)
            {
                return NotFound(new { error = "Responder not found" });
            }

            var startTime = DateTime.UtcNow.AddHours(-hours);
            var trackingData = _locationUpdates
                .Where(l => l.ResponderId == responderId && l.Timestamp >= startTime)
                .OrderBy(l => l.Timestamp)
                .Select(l => new
                {
                    timestamp = l.Timestamp,
                    coordinates = l.Coordinates,
                    address = l.Address,
                    speed = l.Speed,
                    heading = l.Heading,
                    accuracy = l.Accuracy
                })
                .ToList();

            return Ok(new
            {
                responder_id = responderId,
                responder_name = responder.Name,
                tracking_data = trackingData,
                total_points = trackingData.Count,
                time_range_hours = hours,
                last_updated = DateTime.UtcNow
            });
        }

        [HttpGet("incidents/nearby")]
        public IActionResult GetNearbyIncidents([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radiusKm = 5.0)
        {
            var incidents = InMemoryStore.GetAllIncidents();
            var nearbyIncidents = new List<object>();

            foreach (var incident in incidents.Where(i => i.Status == "active"))
            {
                var incidentCoords = GetCoordinatesForLocation(incident.Location);
                var distance = CalculateDistance(lat, lng, incidentCoords.Latitude, incidentCoords.Longitude);

                if (distance <= radiusKm)
                {
                    nearbyIncidents.Add(new
                    {
                        incident_id = incident.Id,
                        type = incident.Type,
                        location = incident.Location,
                        coordinates = incidentCoords,
                        priority = incident.Priority,
                        distance_km = Math.Round(distance, 2),
                        description = incident.Description
                    });
                }
            }

            return Ok(new
            {
                nearby_incidents = nearbyIncidents.OrderBy(i => ((dynamic)i).distance_km),
                total_nearby = nearbyIncidents.Count,
                search_radius_km = radiusKm,
                search_coordinates = new { lat, lng },
                last_updated = DateTime.UtcNow
            });
        }

        [HttpPost("geo-fences")]
        public IActionResult CreateGeoFence([FromBody] CreateGeoFenceRequest request)
        {
            var geoFence = new GeoFence
            {
                Id = _nextGeoFenceId++,
                Name = request.Name,
                Description = request.Description,
                Coordinates = request.Coordinates,
                RadiusMeters = request.RadiusMeters,
                AlertMessage = request.AlertMessage,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _geoFences.Add(geoFence);

            return CreatedAtAction(nameof(GetGeoFence), new { id = geoFence.Id }, geoFence);
        }

        [HttpGet("geo-fences")]
        public IActionResult GetGeoFences()
        {
            return Ok(new
            {
                geo_fences = _geoFences.Where(g => g.IsActive),
                total_count = _geoFences.Count(g => g.IsActive),
                last_updated = DateTime.UtcNow
            });
        }

        [HttpGet("geo-fences/{id}")]
        public IActionResult GetGeoFence(int id)
        {
            var geoFence = _geoFences.FirstOrDefault(g => g.Id == id);
            if (geoFence == null)
            {
                return NotFound(new { error = "Geo-fence not found" });
            }

            return Ok(geoFence);
        }

        [HttpPut("geo-fences/{id}/toggle")]
        public IActionResult ToggleGeoFence(int id)
        {
            var geoFence = _geoFences.FirstOrDefault(g => g.Id == id);
            if (geoFence == null)
            {
                return NotFound(new { error = "Geo-fence not found" });
            }

            geoFence.IsActive = !geoFence.IsActive;
            geoFence.UpdatedAt = DateTime.UtcNow;

            return Ok(geoFence);
        }

        [HttpGet("optimization/routes")]
        public IActionResult GetOptimizedRoutes([FromQuery] double lat, [FromQuery] double lng, [FromQuery] int maxResponders = 5)
        {
            var incidents = InMemoryStore.GetAllIncidents().Where(i => i.Status == "active").ToList();
            var responders = InMemoryStore.GetAllResponders().Where(r => r.Status == "available").ToList();

            var optimizedRoutes = new List<object>();

            foreach (var incident in incidents.Take(3)) // Limit to 3 incidents for demo
            {
                var incidentCoords = GetCoordinatesForLocation(incident.Location);
                var nearbyResponders = responders
                    .Select(r => new
                    {
                        responder = r,
                        distance = CalculateDistance(lat, lng, incidentCoords.Latitude, incidentCoords.Longitude)
                    })
                    .OrderBy(r => r.distance)
                    .Take(maxResponders)
                    .ToList();

                optimizedRoutes.Add(new
                {
                    incident_id = incident.Id,
                    incident_type = incident.Type,
                    incident_location = incident.Location,
                    incident_coordinates = incidentCoords,
                    priority = incident.Priority,
                    recommended_responders = nearbyResponders.Select(r => new
                    {
                        responder_id = r.responder.Id,
                        responder_name = r.responder.Name,
                        role = r.responder.Role,
                        distance_km = Math.Round(r.distance, 2),
                        estimated_arrival_minutes = Math.Round(r.distance * 2, 1) // Rough estimate
                    }).ToList()
                });
            }

            return Ok(new
            {
                optimized_routes = optimizedRoutes,
                total_routes = optimizedRoutes.Count,
                search_coordinates = new { lat, lng },
                last_updated = DateTime.UtcNow
            });
        }

        private Coordinates GetDefaultCoordinates(string location)
        {
            // Mock coordinates based on location string
            if (location.Contains("Downtown")) return new Coordinates { Latitude = 27.9506, Longitude = -82.4572 };
            if (location.Contains("North")) return new Coordinates { Latitude = 28.0587, Longitude = -82.4572 };
            if (location.Contains("South")) return new Coordinates { Latitude = 27.9425, Longitude = -82.4572 };
            if (location.Contains("West")) return new Coordinates { Latitude = 27.9506, Longitude = -82.5000 };
            if (location.Contains("East")) return new Coordinates { Latitude = 27.9506, Longitude = -82.4000 };
            return new Coordinates { Latitude = 27.9506, Longitude = -82.4572 };
        }

        private Coordinates GetCoordinatesForLocation(string location)
        {
            return GetDefaultCoordinates(location);
        }

        private bool IsResponderOnline(int responderId)
        {
            var lastUpdate = _locationUpdates
                .Where(l => l.ResponderId == responderId)
                .OrderByDescending(l => l.Timestamp)
                .FirstOrDefault();

            return lastUpdate != null && DateTime.UtcNow.Subtract(lastUpdate.Timestamp).TotalMinutes < 15;
        }

        private List<object> CheckGeoFenceAlerts(int responderId, Coordinates coordinates)
        {
            var alerts = new List<object>();
            var activeGeoFences = _geoFences.Where(g => g.IsActive);

            foreach (var geoFence in activeGeoFences)
            {
                var distance = CalculateDistance(
                    coordinates.Latitude, coordinates.Longitude,
                    geoFence.Coordinates.Latitude, geoFence.Coordinates.Longitude);

                if (distance <= geoFence.RadiusMeters / 1000.0) // Convert meters to km
                {
                    alerts.Add(new
                    {
                        geo_fence_id = geoFence.Id,
                        geo_fence_name = geoFence.Name,
                        alert_message = geoFence.AlertMessage,
                        distance_km = Math.Round(distance, 2),
                        timestamp = DateTime.UtcNow
                    });
                }
            }

            return alerts;
        }

        private double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371; // Earth's radius in kilometers
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLng = (lng2 - lng1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }

    public class UpdateLocationRequest
    {
        public Coordinates Coordinates { get; set; } = new();
        public string? Address { get; set; }
        public double? Speed { get; set; }
        public double? Heading { get; set; }
        public double? Accuracy { get; set; }
    }

    public class CreateGeoFenceRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Coordinates Coordinates { get; set; } = new();
        public double RadiusMeters { get; set; }
        public string AlertMessage { get; set; } = string.Empty;
    }

    public class Coordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class LocationUpdate
    {
        public int Id { get; set; }
        public int ResponderId { get; set; }
        public Coordinates Coordinates { get; set; } = new();
        public string Address { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double? Speed { get; set; }
        public double? Heading { get; set; }
        public double? Accuracy { get; set; }
    }

    public class GeoFence
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Coordinates Coordinates { get; set; } = new();
        public double RadiusMeters { get; set; }
        public string AlertMessage { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 