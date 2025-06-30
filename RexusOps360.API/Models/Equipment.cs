using System.ComponentModel.DataAnnotations;

namespace RexusOps360.API.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Type is required")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty; // "Medical", "Transport", "Communication", "Safety", "Other"
        
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        
        [Required(ErrorMessage = "Available quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Available quantity cannot be negative")]
        public int AvailableQuantity { get; set; }
        
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string Location { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Available|InUse|Maintenance|Retired)$", ErrorMessage = "Status must be Available, InUse, Maintenance, or Retired")]
        public string Status { get; set; } = "Available";
        
        public DateTime? LastMaintenance { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 