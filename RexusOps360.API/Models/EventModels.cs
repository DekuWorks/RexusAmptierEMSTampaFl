using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RexusOps360.API.Models
{
    #region Event Management Models

    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public EventType Type { get; set; }

        [Required]
        public EventStatus Status { get; set; }

        [StringLength(500)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? VirtualMeetingUrl { get; set; }

        [StringLength(100)]
        public string? TimeZone { get; set; }

        public int MaxAttendees { get; set; }

        public decimal? RegistrationFee { get; set; }

        [StringLength(50)]
        public string? Currency { get; set; } = "USD";

        public bool IsPublic { get; set; } = true;

        public bool RequiresApproval { get; set; } = false;

        [StringLength(500)]
        public string? BrandingLogoUrl { get; set; }

        [StringLength(100)]
        public string? BrandingColor { get; set; }

        [StringLength(1000)]
        public string? CustomCss { get; set; }



        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public virtual ICollection<EventSpeaker> Speakers { get; set; } = new List<EventSpeaker>();
        public virtual ICollection<EventSponsor> Sponsors { get; set; } = new List<EventSponsor>();
    }

    public class Session
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? VirtualMeetingUrl { get; set; }

        public int MaxCapacity { get; set; }

        [StringLength(100)]
        public string? Track { get; set; }

        public SessionType Type { get; set; }

        public SessionStatus Status { get; set; }

        [StringLength(1000)]
        public string? Materials { get; set; }

        // Navigation properties
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;
        public virtual ICollection<SessionSpeaker> Speakers { get; set; } = new List<SessionSpeaker>();
        public virtual ICollection<SessionAttendee> Attendees { get; set; } = new List<SessionAttendee>();
    }

    public class Speaker
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(200)]
        public string? Organization { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        [StringLength(500)]
        public string? LinkedInUrl { get; set; }

        [StringLength(500)]
        public string? TwitterUrl { get; set; }



        // Navigation properties
        public virtual ICollection<EventSpeaker> Events { get; set; } = new List<EventSpeaker>();
        public virtual ICollection<SessionSpeaker> Sessions { get; set; } = new List<SessionSpeaker>();
    }

    public class Registration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Organization { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        public RegistrationStatus Status { get; set; }

        public RegistrationType Type { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public DateTime? CheckInDate { get; set; }

        [StringLength(500)]
        public string? SpecialRequirements { get; set; }

        [StringLength(500)]
        public string? DietaryRestrictions { get; set; }

        public bool EmailNotifications { get; set; } = true;

        public bool SmsNotifications { get; set; } = false;

        [StringLength(20)]
        public string? SmsPhone { get; set; }

        // Payment information
        public decimal? AmountPaid { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? TransactionId { get; set; }

        public DateTime? PaymentDate { get; set; }

        // Navigation properties
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;
        public virtual ICollection<SessionAttendee> Sessions { get; set; } = new List<SessionAttendee>();
    }

    #endregion

    #region Supporting Models

    public class EventSpeaker
    {
        public int EventId { get; set; }
        public int SpeakerId { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsKeynote { get; set; } = false;

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;

        [ForeignKey("SpeakerId")]
        public virtual Speaker Speaker { get; set; } = null!;
    }

    public class SessionSpeaker
    {
        public int SessionId { get; set; }
        public int SpeakerId { get; set; }
        public string Role { get; set; } = string.Empty;

        [ForeignKey("SessionId")]
        public virtual Session Session { get; set; } = null!;

        [ForeignKey("SpeakerId")]
        public virtual Speaker Speaker { get; set; } = null!;
    }

    public class SessionAttendee
    {
        public int SessionId { get; set; }
        public int RegistrationId { get; set; }
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
        public bool Attended { get; set; } = false;

        [ForeignKey("SessionId")]
        public virtual Session Session { get; set; } = null!;

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; } = null!;
    }

    public class EventSponsor
    {
        public int EventId { get; set; }
        public int SponsorId { get; set; }
        public string SponsorshipLevel { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime SponsorshipDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;

        [ForeignKey("SponsorId")]
        public virtual Sponsor Sponsor { get; set; } = null!;
    }

    public class Sponsor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(200)]
        public string? ContactEmail { get; set; }

        [StringLength(20)]
        public string? ContactPhone { get; set; }

        // Multi-tenant support
        [Required]


        // Navigation properties
        public virtual ICollection<EventSponsor> Events { get; set; } = new List<EventSponsor>();
    }

    #endregion

    #region Request/Response Models

    public class CreateEventRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public EventType Type { get; set; }

        [StringLength(500)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? VirtualMeetingUrl { get; set; }

        [StringLength(100)]
        public string? TimeZone { get; set; }

        public int MaxAttendees { get; set; }

        public decimal? RegistrationFee { get; set; }

        [StringLength(50)]
        public string? Currency { get; set; } = "USD";

        public bool IsPublic { get; set; } = true;

        public bool RequiresApproval { get; set; } = false;

        [StringLength(500)]
        public string? BrandingLogoUrl { get; set; }

        [StringLength(100)]
        public string? BrandingColor { get; set; }

        [StringLength(1000)]
        public string? CustomCss { get; set; }
    }

    public class UpdateEventRequest
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public EventType? Type { get; set; }

        [StringLength(500)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? VirtualMeetingUrl { get; set; }

        [StringLength(100)]
        public string? TimeZone { get; set; }

        public int? MaxAttendees { get; set; }

        public decimal? RegistrationFee { get; set; }

        [StringLength(50)]
        public string? Currency { get; set; }

        public bool? IsPublic { get; set; }

        public bool? RequiresApproval { get; set; }

        [StringLength(500)]
        public string? BrandingLogoUrl { get; set; }

        [StringLength(100)]
        public string? BrandingColor { get; set; }

        [StringLength(1000)]
        public string? CustomCss { get; set; }
    }

    public class CreateRegistrationRequest
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Organization { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        public RegistrationType Type { get; set; } = RegistrationType.Individual;

        [StringLength(500)]
        public string? SpecialRequirements { get; set; }

        [StringLength(500)]
        public string? DietaryRestrictions { get; set; }

        public bool EmailNotifications { get; set; } = true;

        public bool SmsNotifications { get; set; } = false;

        [StringLength(20)]
        public string? SmsPhone { get; set; }

        public List<int> SessionIds { get; set; } = new List<int>();
    }

    public class CreateSessionRequest
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? VirtualMeetingUrl { get; set; }

        public int MaxCapacity { get; set; }

        [StringLength(100)]
        public string? Track { get; set; }

        public SessionType Type { get; set; }

        [StringLength(1000)]
        public string? Materials { get; set; }

        public List<int> SpeakerIds { get; set; } = new List<int>();
    }

    public class EventResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public EventType Type { get; set; }
        public EventStatus Status { get; set; }
        public string? Location { get; set; }
        public string? VirtualMeetingUrl { get; set; }
        public string? TimeZone { get; set; }
        public int MaxAttendees { get; set; }
        public decimal? RegistrationFee { get; set; }
        public string? Currency { get; set; }
        public bool IsPublic { get; set; }
        public bool RequiresApproval { get; set; }
        public string? BrandingLogoUrl { get; set; }
        public string? BrandingColor { get; set; }
        public string? CustomCss { get; set; }
        public int RegistrationCount { get; set; }
        public int SessionCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<SessionResponse> Sessions { get; set; } = new List<SessionResponse>();
        public List<SpeakerResponse> Speakers { get; set; } = new List<SpeakerResponse>();
    }

    public class SessionResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Location { get; set; }
        public string? VirtualMeetingUrl { get; set; }
        public int MaxCapacity { get; set; }
        public string? Track { get; set; }
        public SessionType Type { get; set; }
        public SessionStatus Status { get; set; }
        public string? Materials { get; set; }
        public int AttendeeCount { get; set; }
        public List<SpeakerResponse> Speakers { get; set; } = new List<SpeakerResponse>();
    }

    public class SpeakerResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Organization { get; set; }
        public string? Bio { get; set; }
        public string? Email { get; set; }
        public string? PhotoUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? TwitterUrl { get; set; }
    }

    public class RegistrationResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Organization { get; set; }
        public string? JobTitle { get; set; }
        public RegistrationStatus Status { get; set; }
        public RegistrationType Type { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? CheckInDate { get; set; }
        public string? SpecialRequirements { get; set; }
        public string? DietaryRestrictions { get; set; }
        public decimal? AmountPaid { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public List<SessionResponse> Sessions { get; set; } = new List<SessionResponse>();
    }

    #endregion

    #region Enums

    public enum EventType
    {
        InPerson,
        Virtual,
        Hybrid
    }

    public enum EventStatus
    {
        Draft,
        Published,
        RegistrationOpen,
        RegistrationClosed,
        InProgress,
        Completed,
        Cancelled
    }

    public enum SessionType
    {
        Keynote,
        Breakout,
        Panel,
        Workshop,
        Networking,
        Lunch,
        Break
    }

    public enum SessionStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled
    }

    public enum RegistrationStatus
    {
        Pending,
        Confirmed,
        CheckedIn,
        Cancelled,
        Waitlisted
    }

    public enum RegistrationType
    {
        Individual,
        Group,
        VIP,
        Speaker,
        Sponsor
    }

    #endregion
} 