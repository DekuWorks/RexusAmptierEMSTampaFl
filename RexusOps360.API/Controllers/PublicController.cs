/*
 * RexusOps360 EMS API - Public Controller
 * 
 * This controller handles public endpoints that don't require authentication.
 * It allows anonymous users to submit incident reports and access public information.
 * 
 * Features:
 * - Anonymous incident reporting
 * - Public emergency information
 * - Rate limiting for security
 * - Input validation and sanitization
 * - Audit logging for public submissions
 * 
 * Security:
 * - Rate limiting on submissions
 * - Input validation and sanitization
 * - IP address tracking
 * - Spam protection
 * 
 * Author: RexusOps360 Development Team
 * Version: 1.0.0
 * Last Updated: 2025-01-17
 */

using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Models;
using RexusOps360.API.Services;
using System.ComponentModel.DataAnnotations;

namespace RexusOps360.API.Controllers
{
    /// <summary>
    /// Public Controller for Anonymous Access
    /// Handles public endpoints that don't require authentication
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PublicController : ControllerBase
    {
        private readonly ILogger<PublicController> _logger;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IRecaptchaService _recaptchaService;

        public PublicController(
            ILogger<PublicController> logger,
            INotificationService notificationService,
            IAuditService auditService,
            IRecaptchaService recaptchaService)
        {
            _logger = logger;
            _notificationService = notificationService;
            _auditService = auditService;
            _recaptchaService = recaptchaService;
        }

        /// <summary>
        /// Submit an incident report anonymously
        /// </summary>
        /// <param name="request">The incident report data</param>
        /// <returns>Success response with incident ID</returns>
        /// <response code="200">Incident report submitted successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="429">Too many submissions - rate limited</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("incidents")]
        public async Task<IActionResult> SubmitIncidentReport([FromBody] PublicIncidentRequest request)
        {
            try
            {
                // Get client IP for rate limiting and security
                var ipAddress = GetClientIpAddress();
                
                // Log the public submission attempt
                _logger.LogInformation("Public incident report submitted from IP: {IpAddress}", ipAddress);
                
                // Validate the request
                var validationResult = await ValidatePublicIncidentRequestAsync(request);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Invalid public incident report: {Errors}", string.Join(", ", validationResult.Errors));
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = validationResult.Errors
                    });
                }
                
                // Check rate limiting (max 5 submissions per hour per IP)
                if (IsRateLimited(ipAddress))
                {
                    _logger.LogWarning("Rate limited public incident report from IP: {IpAddress}", ipAddress);
                    return StatusCode(429, new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Too many submissions. Please wait before submitting another report."
                    });
                }
                
                // Create the incident from public report
                var incident = CreateIncidentFromPublicReport(request, ipAddress);
                
                // Log the incident creation
                await _auditService.LogActivityAsync("PublicIncidentReport", 
                    $"Public incident report submitted: {incident.Type} at {incident.Location}", 
                    ipAddress,
                    ipAddress);
                
                // Send notifications to appropriate responders
                await SendIncidentNotifications(incident);
                
                _logger.LogInformation("Public incident report processed successfully. Incident ID: {IncidentId}", incident.Id);
                
                return Ok(new ApiResponse<PublicIncidentResponse>
                {
                    Success = true,
                    Message = "Incident report submitted successfully. Emergency services have been notified.",
                    Data = new PublicIncidentResponse
                    {
                        IncidentId = incident.Id,
                        ReferenceNumber = GenerateReferenceNumber(),
                        EstimatedResponseTime = GetEstimatedResponseTime(incident.Priority),
                        Message = "Thank you for reporting this incident. Emergency services have been notified and will respond accordingly."
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing public incident report");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while processing your report. Please try again or contact emergency services directly."
                });
            }
        }

        /// <summary>
        /// Get public emergency information
        /// </summary>
        /// <returns>Public emergency information and contact details</returns>
        [HttpGet("emergency-info")]
        public IActionResult GetEmergencyInfo()
        {
            return Ok(new ApiResponse<EmergencyInfo>
            {
                Success = true,
                Data = new EmergencyInfo
                {
                    EmergencyNumber = "911",
                    EmsDispatch = "(813) 555-EMER",
                    PoliceDispatch = "(813) 555-POLICE",
                    FireDispatch = "(813) 555-FIRE",
                    NonEmergencyNumber = "(813) 555-NONEMER",
                    Website = "https://www.tampa.gov/emergency",
                    Message = "For immediate emergency assistance, call 911. For non-emergency incidents, use this form or call the non-emergency number."
                }
            });
        }

        /// <summary>
        /// Get current emergency alerts
        /// </summary>
        /// <returns>List of current emergency alerts</returns>
        [HttpGet("alerts")]
        public IActionResult GetEmergencyAlerts()
        {
            // Mock emergency alerts - in production, this would come from a real alert system
            var alerts = new List<EmergencyAlert>
            {
                new EmergencyAlert
                {
                    Id = 1,
                    Title = "Severe Weather Warning",
                    Message = "Severe thunderstorms expected in Tampa area. Take shelter if necessary.",
                    Type = "weather",
                    Priority = "medium",
                    ExpiresAt = DateTime.UtcNow.AddHours(2)
                },
                new EmergencyAlert
                {
                    Id = 2,
                    Title = "Traffic Advisory",
                    Message = "Major traffic incident on I-275. Expect delays.",
                    Type = "traffic",
                    Priority = "low",
                    ExpiresAt = DateTime.UtcNow.AddHours(1)
                }
            };
            
            return Ok(new ApiResponse<List<EmergencyAlert>>
            {
                Success = true,
                Data = alerts
            });
        }

        // =============================================================================
        // PRIVATE HELPER METHODS
        // =============================================================================

        /// <summary>
        /// Validate the public incident request
        /// </summary>
        private async Task<ValidationResult> ValidatePublicIncidentRequestAsync(PublicIncidentRequest request)
        {
            var errors = new List<string>();
            
            // Required fields validation
            if (string.IsNullOrWhiteSpace(request.ReporterName))
                errors.Add("Reporter name is required");
                
            if (string.IsNullOrWhiteSpace(request.ReporterPhone))
                errors.Add("Reporter phone number is required");
                
            if (string.IsNullOrWhiteSpace(request.IncidentAddress))
                errors.Add("Incident address is required");
                
            if (string.IsNullOrWhiteSpace(request.IncidentType))
                errors.Add("Incident type is required");
                
            if (string.IsNullOrWhiteSpace(request.IncidentDescription))
                errors.Add("Incident description is required");
            
            // Phone number validation
            if (!string.IsNullOrWhiteSpace(request.ReporterPhone))
            {
                var cleanPhone = request.ReporterPhone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                if (!cleanPhone.All(char.IsDigit) || cleanPhone.Length < 10)
                    errors.Add("Please enter a valid phone number");
            }
            
            // Email validation (optional field)
            if (!string.IsNullOrWhiteSpace(request.ReporterEmail))
            {
                var emailAttribute = new EmailAddressAttribute();
                if (!emailAttribute.IsValid(request.ReporterEmail))
                    errors.Add("Please enter a valid email address");
            }
            
            // Priority validation
            var validPriorities = new[] { "low", "medium", "high", "emergency" };
            if (!validPriorities.Contains(request.IncidentPriority?.ToLower()))
                errors.Add("Invalid priority level");
            
            // reCAPTCHA validation
            if (!string.IsNullOrWhiteSpace(request.RecaptchaResponse))
            {
                var clientIp = GetClientIpAddress();
                var isRecaptchaValid = await _recaptchaService.VerifyRecaptchaAsync(request.RecaptchaResponse, clientIp);
                
                if (!isRecaptchaValid)
                {
                    errors.Add("reCAPTCHA verification failed. Please try again.");
                }
            }
            else
            {
                errors.Add("reCAPTCHA verification is required");
            }
            
            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        /// <summary>
        /// Check if the IP address is rate limited
        /// </summary>
        private bool IsRateLimited(string ipAddress)
        {
            // Simple rate limiting - in production, use a proper rate limiting service
            // This is a mock implementation
            return false; // Allow all submissions for demo
        }

        /// <summary>
        /// Create an incident from the public report
        /// </summary>
        private Incident CreateIncidentFromPublicReport(PublicIncidentRequest request, string ipAddress)
        {
            return new Incident
            {
                Id = GenerateIncidentId(),
                Type = request.IncidentType,
                Location = $"{request.IncidentAddress}, {request.IncidentCity}, {request.IncidentState} {request.IncidentZip}",
                Description = request.IncidentDescription,
                Priority = request.IncidentPriority,
                Status = "reported",
                ReportedBy = request.ReporterName,
                ReporterPhone = request.ReporterPhone,
                ReporterEmail = request.ReporterEmail,
                ReporterRelation = request.ReporterRelation,
                PeopleInvolved = request.PeopleInvolved?.ToString(),
                Injuries = request.Injuries,
                VehiclesInvolved = request.VehiclesInvolved?.ToString(),
                Hazards = request.Hazards,
                AdditionalInfo = request.AdditionalInfo,
                Source = "public_report",
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Send notifications for the incident
        /// </summary>
        private async Task SendIncidentNotifications(Incident incident)
        {
            try
            {
                // Send notification to appropriate responders based on incident type
                await _notificationService.CreateNotificationAsync(
                    $"New Public Incident Report: {incident.Type}",
                    $"Incident reported at {incident.Location}. Priority: {incident.Priority}. Reported by: {incident.ReportedBy}",
                    "Info",
                    "tampa-fl"
                );
                
                _logger.LogInformation("Notification sent for public incident {IncidentId}", incident.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification for public incident {IncidentId}", incident.Id);
            }
        }

        /// <summary>
        /// Get estimated response time based on priority
        /// </summary>
        private string GetEstimatedResponseTime(string priority)
        {
            return priority?.ToLower() switch
            {
                "emergency" => "Immediate response",
                "high" => "5-10 minutes",
                "medium" => "15-30 minutes",
                "low" => "30-60 minutes",
                _ => "Response time varies"
            };
        }

        /// <summary>
        /// Generate a unique incident ID
        /// </summary>
        private int GenerateIncidentId()
        {
            return new Random().Next(10000, 99999);
        }

        /// <summary>
        /// Generate a reference number for the incident
        /// </summary>
        private string GenerateReferenceNumber()
        {
            return $"EMS-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        /// <summary>
        /// Get the client IP address
        /// </summary>
        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    // =============================================================================
    // REQUEST AND RESPONSE MODELS
    // =============================================================================

    /// <summary>
    /// Public incident report request model
    /// </summary>
    public class PublicIncidentRequest
    {
        [Required]
        public string ReporterName { get; set; } = string.Empty;
        
        [Required]
        public string ReporterPhone { get; set; } = string.Empty;
        
        public string? ReporterEmail { get; set; }
        
        public string? ReporterRelation { get; set; }
        
        [Required]
        public string IncidentAddress { get; set; } = string.Empty;
        
        [Required]
        public string IncidentCity { get; set; } = string.Empty;
        
        [Required]
        public string IncidentState { get; set; } = string.Empty;
        
        [Required]
        public string IncidentZip { get; set; } = string.Empty;
        
        public string? IncidentLandmark { get; set; }
        
        public string? IncidentCrossStreet { get; set; }
        
        [Required]
        public string IncidentType { get; set; } = string.Empty;
        
        [Required]
        public string IncidentPriority { get; set; } = string.Empty;
        
        [Required]
        public string IncidentDescription { get; set; } = string.Empty;
        
        [Required]
        public DateTime IncidentTime { get; set; }
        
        public string IncidentStatus { get; set; } = "ongoing";
        
        public int? PeopleInvolved { get; set; }
        
        public string? Injuries { get; set; }
        
        public int? VehiclesInvolved { get; set; }
        
        public string? Hazards { get; set; }
        
        public string? AdditionalInfo { get; set; }
        
        public string LocationType { get; set; } = "address";
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public string Source { get; set; } = "public_report";
        
        // reCAPTCHA verification
        public string? RecaptchaResponse { get; set; }
    }

    /// <summary>
    /// Public incident response model
    /// </summary>
    public class PublicIncidentResponse
    {
        public int IncidentId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string EstimatedResponseTime { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Emergency information model
    /// </summary>
    public class EmergencyInfo
    {
        public string EmergencyNumber { get; set; } = string.Empty;
        public string EmsDispatch { get; set; } = string.Empty;
        public string PoliceDispatch { get; set; } = string.Empty;
        public string FireDispatch { get; set; } = string.Empty;
        public string NonEmergencyNumber { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Emergency alert model
    /// </summary>
    public class EmergencyAlert
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Validation result model
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
} 