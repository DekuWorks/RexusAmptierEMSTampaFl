using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Data;
using RexusOps360.API.Models;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentManagementController : ControllerBase
    {
        private static readonly List<MaintenanceSchedule> _maintenanceSchedules = new();
        private static readonly List<EquipmentAlert> _equipmentAlerts = new();
        private static readonly List<BarcodeScan> _barcodeScans = new();
        private static int _nextMaintenanceId = 1;
        private static int _nextAlertId = 1;
        private static int _nextScanId = 1;

        [HttpGet("inventory")]
        public IActionResult GetInventoryStatus()
        {
            var equipment = InMemoryStore.GetAllEquipment();
            var inventoryStatus = new List<object>();

            foreach (var item in equipment)
            {
                var lastMaintenance = _maintenanceSchedules
                    .Where(m => m.EquipmentId == item.Id)
                    .OrderByDescending(m => m.ScheduledDate)
                    .FirstOrDefault();

                var nextMaintenance = _maintenanceSchedules
                    .Where(m => m.EquipmentId == item.Id && m.ScheduledDate > DateTime.UtcNow)
                    .OrderBy(m => m.ScheduledDate)
                    .FirstOrDefault();

                var utilizationRate = item.Quantity > 0 ? (double)(item.Quantity - item.AvailableQuantity) / item.Quantity * 100 : 0;
                var isLowStock = item.AvailableQuantity <= item.Quantity * 0.2; // 20% threshold
                var isMaintenanceDue = IsMaintenanceDue(item.LastMaintenance);

                inventoryStatus.Add(new
                {
                    equipment_id = item.Id,
                    name = item.Name,
                    type = item.Type,
                    total_quantity = item.Quantity,
                    available_quantity = item.AvailableQuantity,
                    utilization_rate = Math.Round(utilizationRate, 2),
                    location = item.Location,
                    status = item.Status,
                    last_maintenance = item.LastMaintenance,
                    next_maintenance = nextMaintenance?.ScheduledDate,
                    is_low_stock = isLowStock,
                    is_maintenance_due = isMaintenanceDue,
                    alerts = GetEquipmentAlerts(item.Id)
                });
            }

            return Ok(new
            {
                inventory_status = inventoryStatus,
                total_equipment = equipment.Count,
                low_stock_items = inventoryStatus.Count(s => s.GetType().GetProperty("is_low_stock")?.GetValue(s) is bool isLowStock && isLowStock),
                maintenance_due_items = inventoryStatus.Count(s => s.GetType().GetProperty("is_maintenance_due")?.GetValue(s) is bool isMaintenanceDue && isMaintenanceDue),
                last_updated = DateTime.UtcNow
            });
        }

        [HttpPost("maintenance/schedule")]
        public IActionResult ScheduleMaintenance([FromBody] ScheduleMaintenanceRequest request)
        {
            var equipment = InMemoryStore.GetAllEquipment().FirstOrDefault(e => e.Id == request.EquipmentId);
            if (equipment == null)
            {
                return NotFound(new { error = "Equipment not found" });
            }

            var maintenanceSchedule = new MaintenanceSchedule
            {
                Id = _nextMaintenanceId++,
                EquipmentId = request.EquipmentId,
                EquipmentName = equipment.Name,
                MaintenanceType = request.MaintenanceType,
                Description = request.Description,
                ScheduledDate = request.ScheduledDate,
                EstimatedDuration = request.EstimatedDuration,
                AssignedTechnician = request.AssignedTechnician,
                Priority = request.Priority ?? "normal",
                Status = "scheduled",
                CreatedAt = DateTime.UtcNow
            };

            _maintenanceSchedules.Add(maintenanceSchedule);

            // Create alert for maintenance
            var alert = new EquipmentAlert
            {
                Id = _nextAlertId++,
                EquipmentId = request.EquipmentId,
                AlertType = "maintenance_scheduled",
                Title = $"Maintenance Scheduled: {equipment.Name}",
                Message = $"Maintenance scheduled for {equipment.Name} on {request.ScheduledDate:MM/dd/yyyy}",
                Priority = request.Priority ?? "normal",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _equipmentAlerts.Add(alert);

            return CreatedAtAction(nameof(GetMaintenanceSchedule), new { id = maintenanceSchedule.Id }, maintenanceSchedule);
        }

        [HttpGet("maintenance/schedule")]
        public IActionResult GetMaintenanceSchedules([FromQuery] string? status = null)
        {
            var schedules = _maintenanceSchedules.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                schedules = schedules.Where(s => s.Status == status);
            }

            var maintenanceSchedules = schedules
                .OrderBy(s => s.ScheduledDate)
                .Select(s => new
                {
                    maintenance_id = s.Id,
                    equipment_id = s.EquipmentId,
                    equipment_name = s.EquipmentName,
                    maintenance_type = s.MaintenanceType,
                    description = s.Description,
                    scheduled_date = s.ScheduledDate,
                    estimated_duration = s.EstimatedDuration,
                    assigned_technician = s.AssignedTechnician,
                    priority = s.Priority,
                    status = s.Status,
                    created_at = s.CreatedAt,
                    completed_at = s.CompletedAt
                })
                .ToList();

            return Ok(new
            {
                maintenance_schedules = maintenanceSchedules,
                total_schedules = maintenanceSchedules.Count,
                pending_schedules = maintenanceSchedules.Count(s => s.status == "scheduled"),
                completed_schedules = maintenanceSchedules.Count(s => s.status == "completed"),
                last_updated = DateTime.UtcNow
            });
        }

        [HttpGet("maintenance/schedule/{id}")]
        public IActionResult GetMaintenanceSchedule(int id)
        {
            var schedule = _maintenanceSchedules.FirstOrDefault(s => s.Id == id);
            if (schedule == null)
            {
                return NotFound(new { error = "Maintenance schedule not found" });
            }

            return Ok(schedule);
        }

        [HttpPut("maintenance/schedule/{id}/complete")]
        public IActionResult CompleteMaintenance(int id, [FromBody] CompleteMaintenanceRequest request)
        {
            var schedule = _maintenanceSchedules.FirstOrDefault(s => s.Id == id);
            if (schedule == null)
            {
                return NotFound(new { error = "Maintenance schedule not found" });
            }

            schedule.Status = "completed";
            schedule.CompletedAt = DateTime.UtcNow;
            schedule.Notes = request.Notes;
            schedule.ActualDuration = request.ActualDuration;

            // Update equipment's last maintenance date
            var equipment = InMemoryStore.GetAllEquipment().FirstOrDefault(e => e.Id == schedule.EquipmentId);
            if (equipment != null)
            {
                equipment.LastMaintenance = DateTime.UtcNow;
            }

            return Ok(schedule);
        }

        [HttpPost("barcode/scan")]
        public IActionResult ScanBarcode([FromBody] BarcodeScanRequest request)
        {
            var equipment = InMemoryStore.GetAllEquipment().FirstOrDefault(e => e.Barcode == request.Barcode);
            if (equipment == null)
            {
                return NotFound(new { error = "Equipment not found for barcode" });
            }

            var scan = new BarcodeScan
            {
                Id = _nextScanId++,
                EquipmentId = equipment.Id,
                Barcode = request.Barcode,
                ScanType = request.ScanType,
                Location = request.Location,
                ScannedBy = request.ScannedBy,
                Timestamp = DateTime.UtcNow,
                Notes = request.Notes
            };

            _barcodeScans.Add(scan);

            // Update equipment status based on scan type
            if (request.ScanType == "checkout")
            {
                if (equipment.AvailableQuantity > 0)
                {
                    equipment.AvailableQuantity--;
                }
            }
            else if (request.ScanType == "checkin")
            {
                if (equipment.AvailableQuantity < equipment.Quantity)
                {
                    equipment.AvailableQuantity++;
                }
            }

            return Ok(new
            {
                scan = scan,
                equipment = new
                {
                    id = equipment.Id,
                    name = equipment.Name,
                    type = equipment.Type,
                    available_quantity = equipment.AvailableQuantity,
                    total_quantity = equipment.Quantity,
                    status = equipment.Status
                },
                scan_successful = true
            });
        }

        [HttpGet("barcode/scans")]
        public IActionResult GetBarcodeScans([FromQuery] int? equipmentId = null, [FromQuery] string? scanType = null)
        {
            var scans = _barcodeScans.AsQueryable();

            if (equipmentId.HasValue)
            {
                scans = scans.Where(s => s.EquipmentId == equipmentId.Value);
            }

            if (!string.IsNullOrEmpty(scanType))
            {
                scans = scans.Where(s => s.ScanType == scanType);
            }

            var scanHistory = scans
                .OrderByDescending(s => s.Timestamp)
                .Select(s => new
                {
                    scan_id = s.Id,
                    equipment_id = s.EquipmentId,
                    barcode = s.Barcode,
                    scan_type = s.ScanType,
                    location = s.Location,
                    scanned_by = s.ScannedBy,
                    timestamp = s.Timestamp,
                    notes = s.Notes
                })
                .ToList();

            return Ok(new
            {
                scan_history = scanHistory,
                total_scans = scanHistory.Count,
                last_updated = DateTime.UtcNow
            });
        }

        [HttpGet("alerts")]
        public IActionResult GetEquipmentAlerts([FromQuery] int? equipmentId = null)
        {
            var alerts = _equipmentAlerts.AsQueryable();

            if (equipmentId.HasValue)
            {
                alerts = alerts.Where(a => a.EquipmentId == equipmentId.Value);
            }

            var activeAlerts = alerts
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    alert_id = a.Id,
                    equipment_id = a.EquipmentId,
                    alert_type = a.AlertType,
                    title = a.Title,
                    message = a.Message,
                    priority = a.Priority,
                    created_at = a.CreatedAt,
                    is_active = a.IsActive
                })
                .ToList();

            return Ok(new
            {
                equipment_alerts = activeAlerts,
                total_alerts = activeAlerts.Count,
                high_priority_alerts = activeAlerts.Count(a => a.priority == "high"),
                last_updated = DateTime.UtcNow
            });
        }

        [HttpPost("alerts")]
        public IActionResult CreateEquipmentAlert([FromBody] CreateEquipmentAlertRequest request)
        {
            var equipment = InMemoryStore.GetAllEquipment().FirstOrDefault(e => e.Id == request.EquipmentId);
            if (equipment == null)
            {
                return NotFound(new { error = "Equipment not found" });
            }

            var alert = new EquipmentAlert
            {
                Id = _nextAlertId++,
                EquipmentId = request.EquipmentId,
                AlertType = request.AlertType,
                Title = request.Title,
                Message = request.Message,
                Priority = request.Priority ?? "normal",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _equipmentAlerts.Add(alert);

            return CreatedAtAction(nameof(GetEquipmentAlerts), new { equipmentId = alert.EquipmentId }, alert);
        }

        [HttpPut("alerts/{id}/dismiss")]
        public IActionResult DismissAlert(int id)
        {
            var alert = _equipmentAlerts.FirstOrDefault(a => a.Id == id);
            if (alert == null)
            {
                return NotFound(new { error = "Alert not found" });
            }

            alert.IsActive = false;
            alert.DismissedAt = DateTime.UtcNow;

            return Ok(alert);
        }

        [HttpGet("analytics/usage")]
        public IActionResult GetEquipmentUsageAnalytics([FromQuery] int days = 30)
        {
            var equipment = InMemoryStore.GetAllEquipment();
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);

            var usageAnalytics = equipment.Select(e => new
            {
                equipment_id = e.Id,
                equipment_name = e.Name,
                equipment_type = e.Type,
                total_quantity = e.Quantity,
                available_quantity = e.AvailableQuantity,
                utilization_rate = e.Quantity > 0 ? Math.Round((double)(e.Quantity - e.AvailableQuantity) / e.Quantity * 100, 2) : 0,
                location = e.Location,
                last_maintenance = e.LastMaintenance,
                days_since_maintenance = e.LastMaintenance.HasValue ? (int)DateTime.UtcNow.Subtract(e.LastMaintenance.Value).TotalDays : 0,
                maintenance_frequency_days = GetMaintenanceFrequency(e.Id),
                scan_count = _barcodeScans.Count(s => s.EquipmentId == e.Id && s.Timestamp >= startDate),
                checkout_count = _barcodeScans.Count(s => s.EquipmentId == e.Id && s.ScanType == "checkout" && s.Timestamp >= startDate),
                checkin_count = _barcodeScans.Count(s => s.EquipmentId == e.Id && s.ScanType == "checkin" && s.Timestamp >= startDate)
            }).ToList();

            return Ok(new
            {
                usage_analytics = usageAnalytics,
                period_days = days,
                total_equipment = equipment.Count,
                high_utilization_items = usageAnalytics.Count(a => a.utilization_rate > 80),
                low_utilization_items = usageAnalytics.Count(a => a.utilization_rate < 20),
                last_updated = DateTime.UtcNow
            });
        }

        private List<object> GetEquipmentAlerts(int equipmentId)
        {
            return _equipmentAlerts
                .Where(a => a.EquipmentId == equipmentId && a.IsActive)
                .Select(a => new
                {
                    alert_id = a.Id,
                    alert_type = a.AlertType,
                    title = a.Title,
                    message = a.Message,
                    priority = a.Priority,
                    created_at = a.CreatedAt
                })
                .Cast<object>()
                .ToList();
        }

        private bool IsMaintenanceDue(DateTime? lastMaintenance)
        {
            if (!lastMaintenance.HasValue) return true;
            return DateTime.UtcNow.Subtract(lastMaintenance.Value).TotalDays > 30;
        }

        private int GetMaintenanceFrequency(int equipmentId)
        {
            var maintenanceHistory = _maintenanceSchedules
                .Where(m => m.EquipmentId == equipmentId && m.Status == "completed")
                .OrderByDescending(m => m.CompletedAt)
                .Take(5)
                .ToList();

            if (maintenanceHistory.Count < 2) return 30; // Default 30 days

            var totalDays = 0;
            for (int i = 1; i < maintenanceHistory.Count; i++)
            {
                totalDays += (int)maintenanceHistory[i - 1].CompletedAt!.Value.Subtract(maintenanceHistory[i].CompletedAt!.Value).TotalDays;
            }

            return totalDays / (maintenanceHistory.Count - 1);
        }
    }

    public class ScheduleMaintenanceRequest
    {
        public int EquipmentId { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public int EstimatedDuration { get; set; } // in hours
        public string? AssignedTechnician { get; set; }
        public string? Priority { get; set; }
    }

    public class CompleteMaintenanceRequest
    {
        public string? Notes { get; set; }
        public int? ActualDuration { get; set; } // in hours
    }

    public class BarcodeScanRequest
    {
        public string Barcode { get; set; } = string.Empty;
        public string ScanType { get; set; } = string.Empty; // checkout, checkin, inventory
        public string? Location { get; set; }
        public string? ScannedBy { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateEquipmentAlertRequest
    {
        public int EquipmentId { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Priority { get; set; }
    }

    public class MaintenanceSchedule
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public int EstimatedDuration { get; set; }
        public string? AssignedTechnician { get; set; }
        public string Priority { get; set; } = "normal";
        public string Status { get; set; } = "scheduled";
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
        public int? ActualDuration { get; set; }
    }

    public class EquipmentAlert
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = "normal";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DismissedAt { get; set; }
    }

    public class BarcodeScan
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ScanType { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? ScannedBy { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Notes { get; set; }
    }
} 