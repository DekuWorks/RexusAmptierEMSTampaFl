using Microsoft.AspNetCore.SignalR;
using RexusOps360.API.Hubs;

namespace RexusOps360.API.Services
{
    public interface IGpsTrackingService
    {
        Task UpdateResponderLocationAsync(string responderId, string location, double? latitude, double? longitude);
        Task<Dictionary<string, ResponderLocation>> GetResponderLocationsAsync();
        Task<ResponderLocation?> GetResponderLocationAsync(string responderId);
        Task<List<ResponderLocation>> GetNearbyRespondersAsync(double latitude, double longitude, double radiusKm);
    }

    public class ResponderLocation
    {
        public string ResponderId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Status { get; set; } = "Available";
    }

    public class GpsTrackingService : IGpsTrackingService
    {
        private readonly IHubContext<EmsHub> _hubContext;
        private static readonly Dictionary<string, ResponderLocation> _responderLocations = new();

        public GpsTrackingService(IHubContext<EmsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task UpdateResponderLocationAsync(string responderId, string location, double? latitude, double? longitude)
        {
            var responderLocation = new ResponderLocation
            {
                ResponderId = responderId,
                Location = location,
                Latitude = latitude,
                Longitude = longitude,
                LastUpdated = DateTime.UtcNow
            };

            _responderLocations[responderId] = responderLocation;

            var locationData = new
            {
                ResponderId = responderId,
                Location = location,
                Latitude = latitude,
                Longitude = longitude,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group("dispatchers").SendAsync("ResponderLocationUpdated", locationData);
            await _hubContext.Clients.Group("admin").SendAsync("ResponderLocationUpdated", locationData);
        }

        public Task<Dictionary<string, ResponderLocation>> GetResponderLocationsAsync()
        {
            return Task.FromResult(_responderLocations);
        }

        public Task<ResponderLocation?> GetResponderLocationAsync(string responderId)
        {
            var location = _responderLocations.TryGetValue(responderId, out var result) ? result : null;
            return Task.FromResult(location);
        }

        public Task<List<ResponderLocation>> GetNearbyRespondersAsync(double latitude, double longitude, double radiusKm)
        {
            var nearbyResponders = new List<ResponderLocation>();

            foreach (var responder in _responderLocations.Values)
            {
                if (responder.Latitude.HasValue && responder.Longitude.HasValue)
                {
                    var distance = CalculateDistance(latitude, longitude, responder.Latitude.Value, responder.Longitude.Value);
                    if (distance <= radiusKm)
                    {
                        nearbyResponders.Add(responder);
                    }
                }
            }

            var result = nearbyResponders.OrderBy(r => 
                CalculateDistance(latitude, longitude, r.Latitude ?? 0, r.Longitude ?? 0)).ToList();
            return Task.FromResult(result);
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