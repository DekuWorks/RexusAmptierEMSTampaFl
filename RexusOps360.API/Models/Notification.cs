using System.ComponentModel.DataAnnotations;

namespace RexusOps360.API.Models
{
    public class Notification
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Type { get; set; } = "Info"; // Info, Warning, Error, Success
        
        [StringLength(50)]
        public string? Area { get; set; } // Geographic area for targeted notifications
        
        [StringLength(50)]
        public string? Priority { get; set; } = "Normal"; // Low, Normal, High, Critical
        
        public bool IsRead { get; set; } = false;
        
        public int? UserId { get; set; } // Target user (null for broadcast)
        

        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ReadAt { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
        
        // Navigation property
        public User? User { get; set; }
    }
} 