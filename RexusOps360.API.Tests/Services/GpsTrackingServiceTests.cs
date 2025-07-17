using Microsoft.AspNetCore.SignalR;
using Moq;
using RexusOps360.API.Hubs;
using RexusOps360.API.Services;
using Xunit;

namespace RexusOps360.API.Tests.Services
{
    public class GpsTrackingServiceTests
    {
        private readonly Mock<IHubContext<EmsHub>> _mockHubContext;
        private readonly GpsTrackingService _service;

        public GpsTrackingServiceTests()
        {
            _mockHubContext = new Mock<IHubContext<EmsHub>>();
            _service = new GpsTrackingService(_mockHubContext.Object);
        }

        [Fact]
        public async Task UpdateResponderLocation_ShouldUpdateLocation()
        {
            // Arrange
            var responderId = "responder1";
            var location = "Downtown Tampa";
            var latitude = 27.9506;
            var longitude = -82.4572;

            // Act
            await _service.UpdateResponderLocationAsync(responderId, location, latitude, longitude);

            // Assert
            var result = await _service.GetResponderLocationAsync(responderId);
            Assert.NotNull(result);
            Assert.Equal(responderId, result.ResponderId);
            Assert.Equal(location, result.Location);
            Assert.Equal(latitude, result.Latitude);
            Assert.Equal(longitude, result.Longitude);
        }

        [Fact]
        public async Task GetResponderLocation_WhenResponderExists_ShouldReturnLocation()
        {
            // Arrange
            var responderId = "responder1";
            var location = "Downtown Tampa";
            var latitude = 27.9506;
            var longitude = -82.4572;

            await _service.UpdateResponderLocationAsync(responderId, location, latitude, longitude);

            // Act
            var result = await _service.GetResponderLocationAsync(responderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responderId, result.ResponderId);
            Assert.Equal(location, result.Location);
        }

        [Fact]
        public async Task GetResponderLocation_WhenResponderDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _service.GetResponderLocationAsync("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetNearbyResponders_ShouldReturnRespondersWithinRadius()
        {
            // Arrange
            var centerLat = 27.9506;
            var centerLon = -82.4572;
            var radius = 10.0; // 10km

            // Add responders at different distances
            await _service.UpdateResponderLocationAsync("responder1", "Downtown", centerLat, centerLon);
            await _service.UpdateResponderLocationAsync("responder2", "Airport", 27.9756, -82.5332); // ~8km away
            await _service.UpdateResponderLocationAsync("responder3", "Beach", 27.9506, -82.4572); // Same location
            await _service.UpdateResponderLocationAsync("responder4", "Far Away", 28.0506, -82.5572); // ~15km away

            // Act
            var nearbyResponders = await _service.GetNearbyRespondersAsync(centerLat, centerLon, radius);

            // Assert
            Assert.Equal(3, nearbyResponders.Count); // Should include responder1, responder2, responder3
            Assert.DoesNotContain(nearbyResponders, r => r.ResponderId == "responder4");
        }

        [Fact]
        public async Task GetResponderLocations_ShouldReturnAllLocations()
        {
            // Arrange
            await _service.UpdateResponderLocationAsync("responder1", "Location1", 27.9506, -82.4572);
            await _service.UpdateResponderLocationAsync("responder2", "Location2", 27.9756, -82.5332);

            // Act
            var locations = await _service.GetResponderLocationsAsync();

            // Assert
            Assert.Equal(2, locations.Count);
            Assert.Contains(locations, kvp => kvp.Key == "responder1");
            Assert.Contains(locations, kvp => kvp.Key == "responder2");
        }
    }
} 