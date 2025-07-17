using RexusOps360.API.Models;
using RexusOps360.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace RexusOps360.API.Services
{
    public interface ISystemIntegrationService
    {
        Task<bool> TestIntegrationAsync(int integrationId);
        Task<List<IntegrationData>> GetIntegrationDataAsync(int integrationId, DateTime? startTime = null);
        Task<bool> SyncScadaDataAsync();
        Task<bool> SyncGpsDataAsync();
        Task<bool> SyncWeatherDataAsync();
        Task<bool> SyncGisDataAsync();
        Task<List<SystemIntegration>> GetActiveIntegrationsAsync(string? type = null);
        Task<SystemIntegration?> CreateIntegrationAsync(SystemIntegration integration);
        Task<bool> UpdateIntegrationAsync(SystemIntegration integration);
    }

    public class SystemIntegrationService : ISystemIntegrationService
    {
        private readonly EmsDbContext _context;
        private readonly ILogger<SystemIntegrationService> _logger;
        private readonly HttpClient _httpClient;
        private readonly INotificationService _notificationService;

        public SystemIntegrationService(
            EmsDbContext context,
            ILogger<SystemIntegrationService> logger,
            HttpClient httpClient,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
            _notificationService = notificationService;
        }

        public async Task<bool> TestIntegrationAsync(int integrationId)
        {
            try
            {
                var integration = await _context.SystemIntegrations.FindAsync(integrationId);
                if (integration == null) return false;

                // Test connection based on integration type
                switch (integration.Type.ToUpper())
                {
                    case "SCADA":
                        return await TestScadaConnectionAsync(integration);
                    case "GPS":
                        return await TestGpsConnectionAsync(integration);
                    case "WEATHER":
                        return await TestWeatherConnectionAsync(integration);
                    case "GIS":
                        return await TestGisConnectionAsync(integration);
                    case "CMMS":
                        return await TestCmmsConnectionAsync(integration);
                    default:
                        return await TestGenericConnectionAsync(integration);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing integration {IntegrationId}", integrationId);
                return false;
            }
        }

        public async Task<List<IntegrationData>> GetIntegrationDataAsync(int integrationId, DateTime? startTime = null)
        {
            var query = _context.IntegrationData.Where(d => d.IntegrationId == integrationId);

            if (startTime.HasValue)
            {
                query = query.Where(d => d.Timestamp >= startTime.Value);
            }

            return await query.OrderByDescending(d => d.Timestamp).ToListAsync();
        }

        public async Task<bool> SyncScadaDataAsync()
        {
            try
            {
                var scadaIntegrations = await _context.SystemIntegrations
                    .Where(i => i.Type == "SCADA" && i.Status == "Active")
                    .ToListAsync();

                foreach (var integration in scadaIntegrations)
                {
                    await SyncScadaIntegrationAsync(integration);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing SCADA data");
                return false;
            }
        }

        public async Task<bool> SyncGpsDataAsync()
        {
            try
            {
                var gpsIntegrations = await _context.SystemIntegrations
                    .Where(i => i.Type == "GPS" && i.Status == "Active")
                    .ToListAsync();

                foreach (var integration in gpsIntegrations)
                {
                    await SyncGpsIntegrationAsync(integration);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing GPS data");
                return false;
            }
        }

        public async Task<bool> SyncWeatherDataAsync()
        {
            try
            {
                var weatherIntegrations = await _context.SystemIntegrations
                    .Where(i => i.Type == "Weather" && i.Status == "Active")
                    .ToListAsync();

                foreach (var integration in weatherIntegrations)
                {
                    await SyncWeatherIntegrationAsync(integration);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing weather data");
                return false;
            }
        }

        public async Task<bool> SyncGisDataAsync()
        {
            try
            {
                var gisIntegrations = await _context.SystemIntegrations
                    .Where(i => i.Type == "GIS" && i.Status == "Active")
                    .ToListAsync();

                foreach (var integration in gisIntegrations)
                {
                    await SyncGisIntegrationAsync(integration);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing GIS data");
                return false;
            }
        }

        public async Task<List<SystemIntegration>> GetActiveIntegrationsAsync(string? type = null)
        {
            var query = _context.SystemIntegrations.Where(i => i.Status == "Active");

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(i => i.Type == type);
            }

            return await query.ToListAsync();
        }

        public async Task<SystemIntegration?> CreateIntegrationAsync(SystemIntegration integration)
        {
            try
            {
                integration.CreatedAt = DateTime.UtcNow;
                integration.Status = "Active";

                _context.SystemIntegrations.Add(integration);
                await _context.SaveChangesAsync();

                return integration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating integration");
                return null;
            }
        }

        public async Task<bool> UpdateIntegrationAsync(SystemIntegration integration)
        {
            try
            {
                integration.UpdatedAt = DateTime.UtcNow;
                _context.SystemIntegrations.Update(integration);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating integration {IntegrationId}", integration.Id);
                return false;
            }
        }

        private async Task<bool> TestScadaConnectionAsync(SystemIntegration integration)
        {
            try
            {
                // Simulate SCADA connection test
                var response = await _httpClient.GetAsync($"{integration.Endpoint}/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing SCADA connection for integration {IntegrationId}", integration.Id);
                return false;
            }
        }

        private async Task<bool> TestGpsConnectionAsync(SystemIntegration integration)
        {
            try
            {
                // Simulate GPS connection test
                var response = await _httpClient.GetAsync($"{integration.Endpoint}/status");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing GPS connection for integration {IntegrationId}", integration.Id);
                return false;
            }
        }

        private async Task<bool> TestWeatherConnectionAsync(SystemIntegration integration)
        {
            try
            {
                // Test weather API connection
                var apiKey = integration.ApiKey ?? "demo";
                var response = await _httpClient.GetAsync($"{integration.Endpoint}?appid={apiKey}&q=Tampa,FL");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing weather connection for integration {IntegrationId}", integration.Id);
                return false;
            }
        }

        private async Task<bool> TestGisConnectionAsync(SystemIntegration integration)
        {
            try
            {
                // Simulate GIS connection test
                var response = await _httpClient.GetAsync($"{integration.Endpoint}/ping");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing GIS connection for integration {IntegrationId}", integration.Id);
                return false;
            }
        }

        private async Task<bool> TestCmmsConnectionAsync(SystemIntegration integration)
        {
            try
            {
                // Simulate CMMS connection test
                var response = await _httpClient.GetAsync($"{integration.Endpoint}/api/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing CMMS connection for integration {IntegrationId}", integration.Id);
                return false;
            }
        }

        private async Task<bool> TestGenericConnectionAsync(SystemIntegration integration)
        {
            try
            {
                var response = await _httpClient.GetAsync(integration.Endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing generic connection for integration {IntegrationId}", integration.Id);
                return false;
            }
        }

        private async Task SyncScadaIntegrationAsync(SystemIntegration integration)
        {
            try
            {
                // Simulate SCADA data sync
                var scadaData = new List<IntegrationData>
                {
                    new IntegrationData
                    {
                        IntegrationId = integration.Id,
                        DataType = "Pressure",
                        DataValue = "45.2",
                        Unit = "PSI",
                        Location = "Pump Station A",
                        Timestamp = DateTime.UtcNow,
                        IsAlert = false,
                        AlertLevel = "Normal"
                    },
                    new IntegrationData
                    {
                        IntegrationId = integration.Id,
                        DataType = "Flow",
                        DataValue = "1200",
                        Unit = "GPM",
                        Location = "Main Line B",
                        Timestamp = DateTime.UtcNow,
                        IsAlert = false,
                        AlertLevel = "Normal"
                    }
                };

                _context.IntegrationData.AddRange(scadaData);
                integration.LastSync = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing SCADA integration {IntegrationId}", integration.Id);
            }
        }

        private async Task SyncGpsIntegrationAsync(SystemIntegration integration)
        {
            try
            {
                // Simulate GPS data sync
                var gpsData = new List<IntegrationData>
                {
                    new IntegrationData
                    {
                        IntegrationId = integration.Id,
                        DataType = "Location",
                        DataValue = "27.9506,-82.4572",
                        Location = "Vehicle 1",
                        Latitude = 27.9506,
                        Longitude = -82.4572,
                        Timestamp = DateTime.UtcNow,
                        IsAlert = false,
                        AlertLevel = "Normal"
                    }
                };

                _context.IntegrationData.AddRange(gpsData);
                integration.LastSync = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing GPS integration {IntegrationId}", integration.Id);
            }
        }

        private async Task SyncWeatherIntegrationAsync(SystemIntegration integration)
        {
            try
            {
                // Simulate weather data sync
                var weatherData = new List<IntegrationData>
                {
                    new IntegrationData
                    {
                        IntegrationId = integration.Id,
                        DataType = "Temperature",
                        DataValue = "85",
                        Unit = "°F",
                        Location = "Tampa, FL",
                        Timestamp = DateTime.UtcNow,
                        IsAlert = false,
                        AlertLevel = "Normal"
                    },
                    new IntegrationData
                    {
                        IntegrationId = integration.Id,
                        DataType = "Precipitation",
                        DataValue = "0.5",
                        Unit = "inches",
                        Location = "Tampa, FL",
                        Timestamp = DateTime.UtcNow,
                        IsAlert = true,
                        AlertLevel = "Warning"
                    }
                };

                _context.IntegrationData.AddRange(weatherData);
                integration.LastSync = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Send weather alert if needed
                var precipitationData = weatherData.FirstOrDefault(d => d.DataType == "Precipitation");
                if (precipitationData?.IsAlert == true)
                {
                    await _notificationService.SendWeatherAlertAsync(
                        $"Heavy precipitation detected: {precipitationData.DataValue} {precipitationData.Unit}",
                        precipitationData.AlertLevel ?? "Warning"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing weather integration {IntegrationId}", integration.Id);
            }
        }

        private async Task SyncGisIntegrationAsync(SystemIntegration integration)
        {
            try
            {
                // Simulate GIS data sync
                var gisData = new List<IntegrationData>
                {
                    new IntegrationData
                    {
                        IntegrationId = integration.Id,
                        DataType = "AssetLocation",
                        DataValue = "Water Main 123",
                        Location = "Downtown Tampa",
                        Latitude = 27.9506,
                        Longitude = -82.4572,
                        Timestamp = DateTime.UtcNow,
                        IsAlert = false,
                        AlertLevel = "Normal"
                    }
                };

                _context.IntegrationData.AddRange(gisData);
                integration.LastSync = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing GIS integration {IntegrationId}", integration.Id);
            }
        }
    }
} 