using System.ComponentModel.DataAnnotations;

namespace RexusOps360.API.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string EntityType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string EntityId { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Details { get; set; }
        
        [StringLength(45)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public bool IsSuccessful { get; set; } = true;
        
        [StringLength(50)]
        public string? Severity { get; set; } = "Info"; // "Info", "Warning", "Error"
    }
} 