using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Data;
using RexusOps360.API.Models;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        [HttpGet("kpi")]
        public IActionResult GetKPIs()
        {
            var incidents = InMemoryStore.GetAllIncidents();
            var responders = InMemoryStore.GetAllResponders();
            var equipment = InMemoryStore.GetAllEquipment();

            // Calculate KPIs
            var totalIncidents = incidents.Count;
            var activeIncidents = incidents.Count(i => i.Status == "active");
            var resolvedIncidents = incidents.Count(i => i.Status == "resolved");
            var highPriorityIncidents = incidents.Count(i => i.Priority == "high" && i.Status == "active");
            
            var totalResponders = responders.Count;
            var availableResponders = responders.Count(r => r.Status == "available");
            var responseRate = totalIncidents > 0 ? (double)resolvedIncidents / totalIncidents * 100 : 0;
            
            var totalEquipment = equipment.Count;
            var availableEquipment = equipment.Count(e => e.Status == "available");
            var equipmentUtilization = totalEquipment > 0 ? (double)(totalEquipment - availableEquipment) / totalEquipment * 100 : 0;

            // Calculate average response time (mock data for now)
            var avgResponseTime = CalculateAverageResponseTime(incidents);

            return Ok(new
            {
                incidents = new
                {
                    total = totalIncidents,
                    active = activeIncidents,
                    resolved = resolvedIncidents,
                    high_priority = highPriorityIncidents,
                    response_rate = Math.Round(responseRate, 2),
                    avg_response_time_minutes = avgResponseTime
                },
                responders = new
                {
                    total = totalResponders,
                    available = availableResponders,
                    utilization_rate = totalResponders > 0 ? Math.Round((double)(totalResponders - availableResponders) / totalResponders * 100, 2) : 0
                },
                equipment = new
                {
                    total = totalEquipment,
                    available = availableEquipment,
                    utilization_rate = Math.Round(equipmentUtilization, 2)
                },
                system_health = new
                {
                    status = "operational",
                    uptime_percentage = 99.8,
                    last_updated = DateTime.UtcNow
                }
            });
        }

        [HttpGet("incidents/heatmap")]
        public IActionResult GetIncidentHeatmap()
        {
            var incidents = InMemoryStore.GetAllIncidents();
            
            // Group incidents by location (simplified for demo)
            var heatmapData = incidents
                .Where(i => i.Status == "active")
                .GroupBy(i => GetLocationArea(i.Location))
                .Select(g => new
                {
                    area = g.Key,
                    incident_count = g.Count(),
                    high_priority_count = g.Count(i => i.Priority == "high"),
                    coordinates = GetCoordinatesForArea(g.Key)
                })
                .ToList();

            return Ok(new
            {
                heatmap_data = heatmapData,
                total_active_incidents = incidents.Count(i => i.Status == "active"),
                last_updated = DateTime.UtcNow
            });
        }

        [HttpGet("incidents/timeline")]
        public IActionResult GetIncidentTimeline([FromQuery] int days = 7)
        {
            var incidents = InMemoryStore.GetAllIncidents();
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);

            var timelineData = new List<object>();
            
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var dayIncidents = incidents.Count(i => i.CreatedAt.Date == date);
                var resolvedIncidents = incidents.Count(i => i.UpdatedAt.Date == date && i.Status == "resolved");
                
                timelineData.Add(new
                {
                    date = date.ToString("yyyy-MM-dd"),
                    new_incidents = dayIncidents,
                    resolved_incidents = resolvedIncidents,
                    active_incidents = incidents.Count(i => i.Status == "active" && i.CreatedAt.Date <= date)
                });
            }

            return Ok(new
            {
                timeline_data = timelineData,
                period_days = days,
                last_updated = DateTime.UtcNow
            });
        }

        [HttpGet("responders/performance")]
        public IActionResult GetResponderPerformance()
        {
            var responders = InMemoryStore.GetAllResponders();
            var incidents = InMemoryStore.GetAllIncidents();

            var performanceData = responders.Select(r => new
            {
                responder_id = r.Id,
                name = r.Name,
                role = r.Role,
                status = r.Status,
                current_location = r.CurrentLocation,
                specializations = r.Specializations,
                assigned_incidents = incidents.Count(i => i.AssignedResponders?.Contains(r.Id) == true),
                response_time_avg = CalculateResponderResponseTime(r.Id, incidents),
                availability_percentage = CalculateAvailabilityPercentage(r.Status)
            }).ToList();

            return Ok(new
            {
                responder_performance = performanceData,
                total_responders = responders.Count,
                available_responders = responders.Count(r => r.Status == "available"),
                last_updated = DateTime.UtcNow
            });
        }

        [HttpGet("equipment/analytics")]
        public IActionResult GetEquipmentAnalytics()
        {
            var equipment = InMemoryStore.GetAllEquipment();

            var equipmentAnalytics = equipment.Select(e => new
            {
                equipment_id = e.Id,
                name = e.Name,
                type = e.Type,
                total_quantity = e.Quantity,
                available_quantity = e.AvailableQuantity,
                utilization_rate = e.Quantity > 0 ? Math.Round((double)(e.Quantity - e.AvailableQuantity) / e.Quantity * 100, 2) : 0,
                location = e.Location,
                status = e.Status,
                last_maintenance = e.LastMaintenance,
                maintenance_due = IsMaintenanceDue(e.LastMaintenance)
            }).ToList();

            return Ok(new
            {
                equipment_analytics = equipmentAnalytics,
                total_equipment = equipment.Count,
                available_equipment = equipment.Count(e => e.Status == "available"),
                maintenance_alerts = equipmentAnalytics.Count(e => e.maintenance_due),
                last_updated = DateTime.UtcNow
            });
        }

        private double CalculateAverageResponseTime(List<Incident> incidents)
        {
            // Mock calculation - in real system, this would use actual response times
            var activeIncidents = incidents.Where(i => i.Status == "active").ToList();
            if (!activeIncidents.Any()) return 0;

            var random = new Random();
            return Math.Round(random.Next(5, 25) + random.NextDouble(), 1);
        }

        private double CalculateResponderResponseTime(int responderId, List<Incident> incidents)
        {
            // Mock calculation
            var random = new Random(responderId);
            return Math.Round(random.Next(3, 20) + random.NextDouble(), 1);
        }

        private double CalculateAvailabilityPercentage(string status)
        {
            return status == "available" ? 100.0 : 0.0;
        }

        private string GetLocationArea(string location)
        {
            // Simplified area extraction
            if (location.Contains("Downtown")) return "Downtown";
            if (location.Contains("North")) return "North Tampa";
            if (location.Contains("South")) return "South Tampa";
            if (location.Contains("West")) return "West Tampa";
            if (location.Contains("East")) return "East Tampa";
            return "Central Tampa";
        }

        private object GetCoordinatesForArea(string area)
        {
            // Mock coordinates for Tampa areas
            return area switch
            {
                "Downtown" => new { lat = 27.9506, lng = -82.4572 },
                "North Tampa" => new { lat = 28.0587, lng = -82.4572 },
                "South Tampa" => new { lat = 27.9425, lng = -82.4572 },
                "West Tampa" => new { lat = 27.9506, lng = -82.5000 },
                "East Tampa" => new { lat = 27.9506, lng = -82.4000 },
                _ => new { lat = 27.9506, lng = -82.4572 }
            };
        }

        private bool IsMaintenanceDue(DateTime? lastMaintenance)
        {
            if (!lastMaintenance.HasValue) return true;
            return DateTime.UtcNow.Subtract(lastMaintenance.Value).TotalDays > 30;
        }
    }
} 