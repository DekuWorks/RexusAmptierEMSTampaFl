using System.ComponentModel.DataAnnotations;

namespace RexusOps360.API.Models
{
    public class Responder
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Role is required")]
        [StringLength(50, ErrorMessage = "Role cannot exceed 50 characters")]
        public string Role { get; set; } = string.Empty; // "Paramedic", "EMT", "Firefighter", "Police Officer", "Dispatcher"
        
        [Required(ErrorMessage = "Contact number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
        public string ContactNumber { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Current location cannot exceed 200 characters")]
        public string CurrentLocation { get; set; } = string.Empty;
        
        public List<string> Specializations { get; set; } = new List<string>();
        
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Available|Busy|Offline)$", ErrorMessage = "Status must be Available, Busy, or Offline")]
        public string Status { get; set; } = "Available";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 