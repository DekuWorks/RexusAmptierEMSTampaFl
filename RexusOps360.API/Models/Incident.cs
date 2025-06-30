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
        public string Priority { get; set; } = string.Empty; // "high", "medium", "low"
        
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Active|Resolved|Pending)$", ErrorMessage = "Status must be Active, Resolved, or Pending")]
        public string Status { get; set; } = "Active"; // "active", "resolved", "closed"
        
        public List<int> AssignedResponders { get; set; } = new();
        
        public List<string> EquipmentNeeded { get; set; } = new();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 