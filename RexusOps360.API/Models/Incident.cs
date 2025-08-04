using System.ComponentModel.DataAnnotations;

namespace RexusOps360.API.Models
{
    public class Incident
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Type is required")]
        [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters")]
        public string Type { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Location is required")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string Location { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Priority is required")]
        [RegularExpression("^(High|Medium|Low)$", ErrorMessage = "Priority must be High, Medium, or Low")]
        public string Priority { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Active|Resolved|Pending)$", ErrorMessage = "Status must be Active, Resolved, or Pending")]
        public string Status { get; set; } = "Active";
        
        // Enhanced fields for clustering and utility differentiation
        [StringLength(50)]
        public string? UtilityType { get; set; } = string.Empty; // "Water", "Sewer", "Combined"
        
        [StringLength(100)]
        public string? Category { get; set; } = string.Empty; // "Sewer Overflow", "Water Main Break", "Flooding"
        
        [StringLength(50)]
        public string? Zone { get; set; } = string.Empty; // Geographic zone for clustering
        
        [StringLength(50)]
        public string? ClusterId { get; set; } = string.Empty; // For grouping similar incidents
        
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }
        
        public int? SeverityLevel { get; set; } // 1-5 scale for impact assessment
        
        [StringLength(500)]
        public string? ContactInfo { get; set; } = string.Empty; // Customer contact information
        
        [StringLength(500)]
        public string? Remarks { get; set; } = string.Empty; // Additional notes from reports
        
        public string AssignedResponders { get; set; } = string.Empty;
        
        public string EquipmentNeeded { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? PhotoPath { get; set; }
        
        [StringLength(100)]
        public string? ReportedBy { get; set; }
        
        // Public incident reporting fields
        [StringLength(20)]
        public string? ReporterPhone { get; set; }
        
        [StringLength(100)]
        public string? ReporterEmail { get; set; }
        
        [StringLength(50)]
        public string? ReporterRelation { get; set; }
        
        [StringLength(200)]
        public string? PeopleInvolved { get; set; }
        
        [StringLength(200)]
        public string? Injuries { get; set; }
        
        [StringLength(200)]
        public string? VehiclesInvolved { get; set; }
        
        [StringLength(200)]
        public string? Hazards { get; set; }
        
        [StringLength(500)]
        public string? AdditionalInfo { get; set; }
        
        [StringLength(50)]
        public string? Source { get; set; } = "Public";
        
        [StringLength(45)]
        public string? IpAddress { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? RespondedAt { get; set; }
        
        // Computed properties for clustering logic
        public bool IsClustered => !string.IsNullOrEmpty(ClusterId);
        
        public bool IsHotspot => SeverityLevel >= 4 || Priority == "High";
        
        // Navigation properties (for Entity Framework)
        public virtual ICollection<Responder>? Responders { get; set; }
    }
} 