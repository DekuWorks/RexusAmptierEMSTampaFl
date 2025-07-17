using System.ComponentModel.DataAnnotations;

namespace RexusOps360.API.Models
{
    public class SystemIntegration
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // "SCADA", "GPS", "GIS", "Weather", "CMMS"
        
        [Required]
        [StringLength(200)]
        public string Endpoint { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? ApiKey { get; set; }
        
        [StringLength(100)]
        public string? Username { get; set; }
        
        [StringLength(100)]
        public string? Password { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // "Active", "Inactive", "Error"
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? Protocol { get; set; } // "REST", "MQTT", "SOAP", "FTP"
        
        public int? PollingIntervalSeconds { get; set; }
        
        public DateTime? LastSync { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class IntegrationData
    {
        public int Id { get; set; }
        
        public int IntegrationId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DataType { get; set; } = string.Empty; // "Pressure", "Flow", "Temperature", "Location"
        
        [Required]
        [StringLength(500)]
        public string DataValue { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Unit { get; set; }
        
        [StringLength(200)]
        public string? Location { get; set; }
        
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public bool IsAlert { get; set; } = false;
        
        [StringLength(50)]
        public string? AlertLevel { get; set; } // "Normal", "Warning", "Critical"
    }
} 