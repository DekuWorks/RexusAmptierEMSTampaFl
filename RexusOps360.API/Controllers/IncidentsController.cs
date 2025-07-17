using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Data;
using RexusOps360.API.Models;
using RexusOps360.API.Services;
using Microsoft.EntityFrameworkCore;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IncidentsController : ControllerBase
    {
        private readonly EmsDbContext _context;
        private readonly IIncidentClusteringService _clusteringService;
        private readonly IHotspotDetectionService _hotspotService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<IncidentsController> _logger;

        public IncidentsController(
            EmsDbContext context,
            IIncidentClusteringService clusteringService,
            IHotspotDetectionService hotspotService,
            INotificationService notificationService,
            ILogger<IncidentsController> logger)
        {
            _context = context;
            _clusteringService = clusteringService;
            _hotspotService = hotspotService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? utilityType = null, [FromQuery] string? status = null)
        {
            try
            {
                var query = _context.Incidents.AsQueryable();

                if (!string.IsNullOrEmpty(utilityType))
                {
                    query = query.Where(i => i.UtilityType == utilityType);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(i => i.Status == status);
                }

                var incidents = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();

                return Ok(new
                {
                    incidents = incidents,
                    count = incidents.Count,
                    utility_types = incidents.Select(i => i.UtilityType).Distinct().ToList(),
                    statuses = incidents.Select(i => i.Status).Distinct().ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving incidents");
                return StatusCode(500, new { error = "Error retrieving incidents" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var incident = await _context.Incidents.FindAsync(id);
                if (incident == null)
                    return NotFound(new { error = "Incident not found" });

                // Get related incidents if this is part of a cluster
                var relatedIncidents = new List<Incident>();
                if (!string.IsNullOrEmpty(incident.ClusterId))
                {
                    relatedIncidents = await _clusteringService.GetClusteredIncidentsAsync(incident.ClusterId);
                }

                return Ok(new
                {
                    incident = incident,
                    related_incidents = relatedIncidents,
                    is_clustered = incident.IsClustered,
                    is_hotspot = incident.IsHotspot
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving incident {IncidentId}", id);
                return StatusCode(500, new { error = "Error retrieving incident" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> Create([FromBody] Incident incident)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { error = "Invalid data provided" });

                // Set default values
                incident.CreatedAt = DateTime.UtcNow;
                incident.Status = incident.Status ?? "Active";
                incident.UtilityType = incident.UtilityType ?? "Combined";

                // Determine severity level based on priority and category
                incident.SeverityLevel = DetermineSeverityLevel(incident.Priority, incident.Category);

                _context.Incidents.Add(incident);
                await _context.SaveChangesAsync();

                // Attempt to cluster the incident
                var clusterId = await _clusteringService.ClusterIncidentAsync(incident);
                if (!string.IsNullOrEmpty(clusterId))
                {
                    _logger.LogInformation($"Incident {incident.Id} clustered with cluster {clusterId}");
                }

                // Check for hotspot creation
                var similarIncidents = await _clusteringService.GetSimilarIncidentsAsync(incident);
                if (similarIncidents.Count >= 3)
                {
                    await _hotspotService.CreateHotspotAsync(incident, similarIncidents);
                }

                // Send notification
                await _notificationService.SendIncidentUpdateAsync(new
                {
                    incident_id = incident.Id,
                    type = incident.Type,
                    location = incident.Location,
                    priority = incident.Priority,
                    utility_type = incident.UtilityType,
                    cluster_id = clusterId
                });

                return CreatedAtAction(nameof(GetById), new { id = incident.Id }, new
                {
                    message = "Incident created successfully",
                    incident = incident,
                    cluster_id = clusterId,
                    is_hotspot = similarIncidents.Count >= 3
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating incident");
                return StatusCode(500, new { error = "Error creating incident" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> Update(int id, [FromBody] Incident updatedIncident)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { error = "Invalid data provided" });

                var existingIncident = await _context.Incidents.FindAsync(id);
                if (existingIncident == null)
                    return NotFound(new { error = "Incident not found" });

                // Update fields
                existingIncident.Type = updatedIncident.Type;
                existingIncident.Location = updatedIncident.Location;
                existingIncident.Description = updatedIncident.Description;
                existingIncident.Priority = updatedIncident.Priority;
                existingIncident.Status = updatedIncident.Status;
                existingIncident.UtilityType = updatedIncident.UtilityType;
                existingIncident.Category = updatedIncident.Category;
                existingIncident.Zone = updatedIncident.Zone;
                existingIncident.SeverityLevel = updatedIncident.SeverityLevel;
                existingIncident.ContactInfo = updatedIncident.ContactInfo;
                existingIncident.Remarks = updatedIncident.Remarks;
                existingIncident.AssignedResponders = updatedIncident.AssignedResponders;
                existingIncident.EquipmentNeeded = updatedIncident.EquipmentNeeded;
                existingIncident.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send notification
                await _notificationService.SendIncidentUpdateAsync(new
                {
                    incident_id = existingIncident.Id,
                    type = existingIncident.Type,
                    location = existingIncident.Location,
                    priority = existingIncident.Priority,
                    status = existingIncident.Status,
                    utility_type = existingIncident.UtilityType
                });

                return Ok(new
                {
                    message = "Incident updated successfully",
                    incident = existingIncident
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating incident {IncidentId}", id);
                return StatusCode(500, new { error = "Error updating incident" });
            }
        }

        [HttpGet("clusters")]
        public async Task<IActionResult> GetClusters([FromQuery] string? utilityType = null)
        {
            try
            {
                var clusters = await _clusteringService.GetIncidentClustersAsync(utilityType);
                return Ok(new { clusters = clusters });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving incident clusters");
                return StatusCode(500, new { error = "Error retrieving incident clusters" });
            }
        }

        [HttpGet("clusters/{clusterId}")]
        public async Task<IActionResult> GetClusterDetails(string clusterId)
        {
            try
            {
                var incidents = await _clusteringService.GetClusteredIncidentsAsync(clusterId);
                if (!incidents.Any())
                    return NotFound(new { error = "Cluster not found" });

                var firstIncident = incidents.First();
                var analytics = new
                {
                    cluster_id = clusterId,
                    total_incidents = incidents.Count,
                    utility_type = firstIncident.UtilityType,
                    category = firstIncident.Category,
                    zone = firstIncident.Zone,
                    first_incident_time = incidents.Min(i => i.CreatedAt),
                    last_incident_time = incidents.Max(i => i.CreatedAt),
                    priority_distribution = incidents.GroupBy(i => i.Priority)
                        .Select(g => new { priority = g.Key, count = g.Count() }),
                    total_contacts = incidents.Count(i => !string.IsNullOrEmpty(i.ContactInfo)),
                    total_remarks = incidents.Count(i => !string.IsNullOrEmpty(i.Remarks))
                };

                return Ok(new
                {
                    cluster = analytics,
                    incidents = incidents
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cluster details for {ClusterId}", clusterId);
                return StatusCode(500, new { error = "Error retrieving cluster details" });
            }
        }

        [HttpGet("utility/{utilityType}")]
        public async Task<IActionResult> GetByUtilityType(string utilityType)
        {
            try
            {
                var incidents = await _context.Incidents
                    .Where(i => i.UtilityType == utilityType)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();

                var analytics = new
                {
                    utility_type = utilityType,
                    total_incidents = incidents.Count,
                    active_incidents = incidents.Count(i => i.Status == "Active"),
                    resolved_incidents = incidents.Count(i => i.Status == "Resolved"),
                    priority_distribution = incidents.GroupBy(i => i.Priority)
                        .Select(g => new { priority = g.Key, count = g.Count() }),
                    category_distribution = incidents.GroupBy(i => i.Category)
                        .Select(g => new { category = g.Key, count = g.Count() })
                };

                return Ok(new
                {
                    analytics = analytics,
                    incidents = incidents
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving incidents for utility type {UtilityType}", utilityType);
                return StatusCode(500, new { error = "Error retrieving incidents" });
            }
        }

        [HttpGet("hotspots")]
        public async Task<IActionResult> GetHotspots([FromQuery] string? utilityType = null)
        {
            try
            {
                var hotspots = await _hotspotService.GetActiveHotspotsAsync(utilityType);
                return Ok(new { hotspots = hotspots });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotspots");
                return StatusCode(500, new { error = "Error retrieving hotspots" });
            }
        }

        private int DetermineSeverityLevel(string priority, string? category)
        {
            var baseLevel = priority switch
            {
                "High" => 4,
                "Medium" => 3,
                "Low" => 2,
                _ => 1
            };

            // Adjust based on category
            var categoryMultiplier = category?.ToLower() switch
            {
                "sewer overflow" => 1.5,
                "water main break" => 1.3,
                "flooding" => 1.4,
                "contamination" => 2.0,
                _ => 1.0
            };

            return Math.Min(5, (int)(baseLevel * categoryMultiplier));
        }
    }
} 