using System.ComponentModel.DataAnnotations;

namespace RexusOps360.API.Models
{
    public class Hotspot
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // "Geographic", "Temporal", "Pattern"
        
        [Required]
        [StringLength(50)]
        public string UtilityType { get; set; } = string.Empty; // "Water", "Sewer", "Combined"
        
        [StringLength(200)]
        public string? Location { get; set; }
        
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }
        
        public double? RadiusMeters { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int IncidentCount { get; set; } = 0;
        
        public int Threshold { get; set; } = 5; // Number of incidents to trigger hotspot
        
        public int TimeWindowMinutes { get; set; } = 30; // Time window for clustering
        
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // "Active", "Resolved", "Monitoring"
        
        public DateTime FirstDetected { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastUpdated { get; set; }
        
        public DateTime? ResolvedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Computed properties
        public bool IsActive => Status == "Active";
        
        public bool IsCritical => Severity == "Critical";
        
        public TimeSpan Duration => DateTime.UtcNow - FirstDetected;
    }
    
    public class HotspotAlert
    {
        public int Id { get; set; }
        
        public int HotspotId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string AlertLevel { get; set; } = string.Empty; // "Info", "Warning", "Critical"
        
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // "Active", "Acknowledged", "Resolved"
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? AcknowledgedAt { get; set; }
        
        [StringLength(100)]
        public string? AcknowledgedBy { get; set; }
        
        public bool IsActive => Status == "Active";
    }
} 