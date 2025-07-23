using RexusOps360.API.Models;
using RexusOps360.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace RexusOps360.API.Services
{
    public interface IEventManagementService
    {
        Task<EventResponse> CreateEventAsync(CreateEventRequest request, string userId);
        Task<EventResponse> UpdateEventAsync(int eventId, UpdateEventRequest request, string userId);
        Task<EventResponse> GetEventAsync(int eventId);
        Task<List<EventResponse>> GetEventsAsync(int tenantId, bool includeDrafts = false);
        Task<bool> DeleteEventAsync(int eventId, string userId);
        
        Task<SessionResponse> CreateSessionAsync(CreateSessionRequest request, string userId);
        Task<SessionResponse> UpdateSessionAsync(int sessionId, CreateSessionRequest request, string userId);
        Task<SessionResponse> GetSessionAsync(int sessionId);
        Task<List<SessionResponse>> GetEventSessionsAsync(int eventId);
        Task<bool> DeleteSessionAsync(int sessionId, string userId);
        
        Task<RegistrationResponse> CreateRegistrationAsync(CreateRegistrationRequest request);
        Task<RegistrationResponse> UpdateRegistrationAsync(int registrationId, CreateRegistrationRequest request);
        Task<RegistrationResponse> GetRegistrationAsync(int registrationId);
        Task<List<RegistrationResponse>> GetEventRegistrationsAsync(int eventId);
        Task<bool> CheckInAttendeeAsync(int registrationId, int sessionId);
        Task<bool> CancelRegistrationAsync(int registrationId);
        
        Task<SpeakerResponse> CreateSpeakerAsync(Speaker speaker, string userId);
        Task<SpeakerResponse> UpdateSpeakerAsync(int speakerId, Speaker speaker, string userId);
        Task<SpeakerResponse> GetSpeakerAsync(int speakerId);
        Task<List<SpeakerResponse>> GetSpeakersAsync(int tenantId);
        Task<bool> DeleteSpeakerAsync(int speakerId, string userId);
        
        Task<bool> AddSpeakerToEventAsync(int eventId, int speakerId, string role, bool isKeynote = false);
        Task<bool> RemoveSpeakerFromEventAsync(int eventId, int speakerId);
        Task<bool> AddSpeakerToSessionAsync(int sessionId, int speakerId, string role);
        Task<bool> RemoveSpeakerFromSessionAsync(int sessionId, int speakerId);
        
        Task<EventAnalytics> GetEventAnalyticsAsync(int eventId);
        Task<RegistrationAnalytics> GetRegistrationAnalyticsAsync(int eventId);
    }

    public class EventManagementService : IEventManagementService
    {
        private readonly EmsDbContext _context;
        private readonly ILogger<EventManagementService> _logger;

        public EventManagementService(EmsDbContext context, ILogger<EventManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Event Management

        public async Task<EventResponse> CreateEventAsync(CreateEventRequest request, string userId)
        {
            try
            {
                var tenantId = await GetTenantIdFromUserAsync(userId);
                
                var newEvent = new Event
                {
                    Title = request.Title,
                    Description = request.Description,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Type = request.Type,
                    Status = EventStatus.Draft,
                    Location = request.Location,
                    VirtualMeetingUrl = request.VirtualMeetingUrl,
                    TimeZone = request.TimeZone ?? "UTC",
                    MaxAttendees = request.MaxAttendees,
                    RegistrationFee = request.RegistrationFee,
                    Currency = request.Currency,
                    IsPublic = request.IsPublic,
                    RequiresApproval = request.RequiresApproval,
                    BrandingLogoUrl = request.BrandingLogoUrl,
                    BrandingColor = request.BrandingColor,
                    CustomCss = request.CustomCss,
                    TenantId = tenantId,
                    CreatedBy = userId,
                    UpdatedBy = userId
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Event created: {EventId} by user {UserId}", newEvent.Id, userId);
                return await GetEventAsync(newEvent.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                throw;
            }
        }

        public async Task<EventResponse> UpdateEventAsync(int eventId, UpdateEventRequest request, string userId)
        {
            try
            {
                var existingEvent = await _context.Events
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (existingEvent == null)
                    throw new ArgumentException("Event not found");

                // Update only provided fields
                if (!string.IsNullOrEmpty(request.Title))
                    existingEvent.Title = request.Title;
                
                if (!string.IsNullOrEmpty(request.Description))
                    existingEvent.Description = request.Description;
                
                if (request.StartDate.HasValue)
                    existingEvent.StartDate = request.StartDate.Value;
                
                if (request.EndDate.HasValue)
                    existingEvent.EndDate = request.EndDate.Value;
                
                if (request.Type.HasValue)
                    existingEvent.Type = request.Type.Value;
                
                if (!string.IsNullOrEmpty(request.Location))
                    existingEvent.Location = request.Location;
                
                if (!string.IsNullOrEmpty(request.VirtualMeetingUrl))
                    existingEvent.VirtualMeetingUrl = request.VirtualMeetingUrl;
                
                if (!string.IsNullOrEmpty(request.TimeZone))
                    existingEvent.TimeZone = request.TimeZone;
                
                if (request.MaxAttendees.HasValue)
                    existingEvent.MaxAttendees = request.MaxAttendees.Value;
                
                if (request.RegistrationFee.HasValue)
                    existingEvent.RegistrationFee = request.RegistrationFee.Value;
                
                if (!string.IsNullOrEmpty(request.Currency))
                    existingEvent.Currency = request.Currency;
                
                if (request.IsPublic.HasValue)
                    existingEvent.IsPublic = request.IsPublic.Value;
                
                if (request.RequiresApproval.HasValue)
                    existingEvent.RequiresApproval = request.RequiresApproval.Value;
                
                if (!string.IsNullOrEmpty(request.BrandingLogoUrl))
                    existingEvent.BrandingLogoUrl = request.BrandingLogoUrl;
                
                if (!string.IsNullOrEmpty(request.BrandingColor))
                    existingEvent.BrandingColor = request.BrandingColor;
                
                if (!string.IsNullOrEmpty(request.CustomCss))
                    existingEvent.CustomCss = request.CustomCss;

                existingEvent.UpdatedAt = DateTime.UtcNow;
                existingEvent.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Event updated: {EventId} by user {UserId}", eventId, userId);
                return await GetEventAsync(eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event {EventId}", eventId);
                throw;
            }
        }

        public async Task<EventResponse> GetEventAsync(int eventId)
        {
            try
            {
                var eventData = await _context.Events
                    .Include(e => e.Sessions)
                    .Include(e => e.Registrations)
                    .Include(e => e.Speakers)
                    .ThenInclude(es => es.Speaker)
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (eventData == null)
                    throw new ArgumentException("Event not found");

                return new EventResponse
                {
                    Id = eventData.Id,
                    Title = eventData.Title,
                    Description = eventData.Description,
                    StartDate = eventData.StartDate,
                    EndDate = eventData.EndDate,
                    Type = eventData.Type,
                    Status = eventData.Status,
                    Location = eventData.Location,
                    VirtualMeetingUrl = eventData.VirtualMeetingUrl,
                    TimeZone = eventData.TimeZone,
                    MaxAttendees = eventData.MaxAttendees,
                    RegistrationFee = eventData.RegistrationFee,
                    Currency = eventData.Currency,
                    IsPublic = eventData.IsPublic,
                    RequiresApproval = eventData.RequiresApproval,
                    BrandingLogoUrl = eventData.BrandingLogoUrl,
                    BrandingColor = eventData.BrandingColor,
                    CustomCss = eventData.CustomCss,
                    RegistrationCount = eventData.Registrations.Count,
                    SessionCount = eventData.Sessions.Count,
                    CreatedAt = eventData.CreatedAt,
                    UpdatedAt = eventData.UpdatedAt,
                    Sessions = eventData.Sessions.Select(s => new SessionResponse
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Description = s.Description,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        Location = s.Location,
                        VirtualMeetingUrl = s.VirtualMeetingUrl,
                        MaxCapacity = s.MaxCapacity,
                        Track = s.Track,
                        Type = s.Type,
                        Status = s.Status,
                        Materials = s.Materials,
                        AttendeeCount = s.Attendees.Count
                    }).ToList(),
                    Speakers = eventData.Speakers.Select(es => new SpeakerResponse
                    {
                        Id = es.Speaker.Id,
                        FirstName = es.Speaker.FirstName,
                        LastName = es.Speaker.LastName,
                        Title = es.Speaker.Title,
                        Organization = es.Speaker.Organization,
                        Bio = es.Speaker.Bio,
                        Email = es.Speaker.Email,
                        PhotoUrl = es.Speaker.PhotoUrl,
                        LinkedInUrl = es.Speaker.LinkedInUrl,
                        TwitterUrl = es.Speaker.TwitterUrl
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event {EventId}", eventId);
                throw;
            }
        }

        public async Task<List<EventResponse>> GetEventsAsync(int tenantId, bool includeDrafts = false)
        {
            try
            {
                var query = _context.Events
                    .Include(e => e.Sessions)
                    .Include(e => e.Registrations)
                    .Where(e => e.TenantId == tenantId);

                if (!includeDrafts)
                    query = query.Where(e => e.Status != EventStatus.Draft);

                var events = await query.ToListAsync();

                return events.Select(e => new EventResponse
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Type = e.Type,
                    Status = e.Status,
                    Location = e.Location,
                    VirtualMeetingUrl = e.VirtualMeetingUrl,
                    TimeZone = e.TimeZone,
                    MaxAttendees = e.MaxAttendees,
                    RegistrationFee = e.RegistrationFee,
                    Currency = e.Currency,
                    IsPublic = e.IsPublic,
                    RequiresApproval = e.RequiresApproval,
                    BrandingLogoUrl = e.BrandingLogoUrl,
                    BrandingColor = e.BrandingColor,
                    CustomCss = e.CustomCss,
                    RegistrationCount = e.Registrations.Count,
                    SessionCount = e.Sessions.Count,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<bool> DeleteEventAsync(int eventId, string userId)
        {
            try
            {
                var eventData = await _context.Events.FindAsync(eventId);
                if (eventData == null)
                    return false;

                _context.Events.Remove(eventData);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Event deleted: {EventId} by user {UserId}", eventId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event {EventId}", eventId);
                throw;
            }
        }

        #endregion

        #region Session Management

        public async Task<SessionResponse> CreateSessionAsync(CreateSessionRequest request, string userId)
        {
            try
            {
                var newSession = new Session
                {
                    EventId = request.EventId,
                    Title = request.Title,
                    Description = request.Description,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Location = request.Location,
                    VirtualMeetingUrl = request.VirtualMeetingUrl,
                    MaxCapacity = request.MaxCapacity,
                    Track = request.Track,
                    Type = request.Type,
                    Status = SessionStatus.Scheduled,
                    Materials = request.Materials
                };

                _context.Sessions.Add(newSession);
                await _context.SaveChangesAsync();

                // Add speakers to session
                foreach (var speakerId in request.SpeakerIds)
                {
                    await AddSpeakerToSessionAsync(newSession.Id, speakerId, "Speaker");
                }

                _logger.LogInformation("Session created: {SessionId} for event {EventId}", newSession.Id, request.EventId);
                return await GetSessionAsync(newSession.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session");
                throw;
            }
        }

        public async Task<SessionResponse> GetSessionAsync(int sessionId)
        {
            try
            {
                var session = await _context.Sessions
                    .Include(s => s.Speakers)
                    .ThenInclude(ss => ss.Speaker)
                    .Include(s => s.Attendees)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);

                if (session == null)
                    throw new ArgumentException("Session not found");

                return new SessionResponse
                {
                    Id = session.Id,
                    Title = session.Title,
                    Description = session.Description,
                    StartTime = session.StartTime,
                    EndTime = session.EndTime,
                    Location = session.Location,
                    VirtualMeetingUrl = session.VirtualMeetingUrl,
                    MaxCapacity = session.MaxCapacity,
                    Track = session.Track,
                    Type = session.Type,
                    Status = session.Status,
                    Materials = session.Materials,
                    AttendeeCount = session.Attendees.Count,
                    Speakers = session.Speakers.Select(ss => new SpeakerResponse
                    {
                        Id = ss.Speaker.Id,
                        FirstName = ss.Speaker.FirstName,
                        LastName = ss.Speaker.LastName,
                        Title = ss.Speaker.Title,
                        Organization = ss.Speaker.Organization,
                        Bio = ss.Speaker.Bio,
                        Email = ss.Speaker.Email,
                        PhotoUrl = ss.Speaker.PhotoUrl,
                        LinkedInUrl = ss.Speaker.LinkedInUrl,
                        TwitterUrl = ss.Speaker.TwitterUrl
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<SessionResponse> UpdateSessionAsync(int sessionId, CreateSessionRequest request, string userId)
        {
            try
            {
                var existingSession = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.Id == sessionId);

                if (existingSession == null)
                    throw new ArgumentException("Session not found");

                existingSession.Title = request.Title;
                existingSession.Description = request.Description;
                existingSession.StartTime = request.StartTime;
                existingSession.EndTime = request.EndTime;
                existingSession.Location = request.Location;
                existingSession.VirtualMeetingUrl = request.VirtualMeetingUrl;
                existingSession.MaxCapacity = request.MaxCapacity;
                existingSession.Track = request.Track;
                existingSession.Type = request.Type;
                existingSession.Materials = request.Materials;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Session updated: {SessionId} by user {UserId}", sessionId, userId);
                return await GetSessionAsync(sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<bool> DeleteSessionAsync(int sessionId, string userId)
        {
            try
            {
                var session = await _context.Sessions.FindAsync(sessionId);
                if (session == null)
                    return false;

                _context.Sessions.Remove(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Session deleted: {SessionId} by user {UserId}", sessionId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<List<SessionResponse>> GetEventSessionsAsync(int eventId)
        {
            try
            {
                var sessions = await _context.Sessions
                    .Include(s => s.Speakers)
                    .ThenInclude(ss => ss.Speaker)
                    .Include(s => s.Attendees)
                    .Where(s => s.EventId == eventId)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();

                return sessions.Select(s => new SessionResponse
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Location = s.Location,
                    VirtualMeetingUrl = s.VirtualMeetingUrl,
                    MaxCapacity = s.MaxCapacity,
                    Track = s.Track,
                    Type = s.Type,
                    Status = s.Status,
                    Materials = s.Materials,
                    AttendeeCount = s.Attendees.Count,
                    Speakers = s.Speakers.Select(ss => new SpeakerResponse
                    {
                        Id = ss.Speaker.Id,
                        FirstName = ss.Speaker.FirstName,
                        LastName = ss.Speaker.LastName,
                        Title = ss.Speaker.Title,
                        Organization = ss.Speaker.Organization,
                        Bio = ss.Speaker.Bio,
                        Email = ss.Speaker.Email,
                        PhotoUrl = ss.Speaker.PhotoUrl,
                        LinkedInUrl = ss.Speaker.LinkedInUrl,
                        TwitterUrl = ss.Speaker.TwitterUrl
                    }).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sessions for event {EventId}", eventId);
                throw;
            }
        }

        #endregion

        #region Registration Management

        public async Task<RegistrationResponse> CreateRegistrationAsync(CreateRegistrationRequest request)
        {
            try
            {
                var newRegistration = new Registration
                {
                    EventId = request.EventId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Organization = request.Organization,
                    JobTitle = request.JobTitle,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    ZipCode = request.ZipCode,
                    Country = request.Country,
                    Status = RegistrationStatus.Pending,
                    Type = request.Type,
                    SpecialRequirements = request.SpecialRequirements,
                    DietaryRestrictions = request.DietaryRestrictions,
                    EmailNotifications = request.EmailNotifications,
                    SmsNotifications = request.SmsNotifications,
                    SmsPhone = request.SmsPhone
                };

                _context.Registrations.Add(newRegistration);
                await _context.SaveChangesAsync();

                // Add sessions to registration
                foreach (var sessionId in request.SessionIds)
                {
                    var sessionAttendee = new SessionAttendee
                    {
                        SessionId = sessionId,
                        RegistrationId = newRegistration.Id
                    };
                    _context.SessionAttendees.Add(sessionAttendee);
                }
                await _context.SaveChangesAsync();

                _logger.LogInformation("Registration created: {RegistrationId} for event {EventId}", newRegistration.Id, request.EventId);
                return await GetRegistrationAsync(newRegistration.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating registration");
                throw;
            }
        }

        public async Task<RegistrationResponse> GetRegistrationAsync(int registrationId)
        {
            try
            {
                var registration = await _context.Registrations
                    .Include(r => r.Sessions)
                    .ThenInclude(sa => sa.Session)
                    .FirstOrDefaultAsync(r => r.Id == registrationId);

                if (registration == null)
                    throw new ArgumentException("Registration not found");

                return new RegistrationResponse
                {
                    Id = registration.Id,
                    FirstName = registration.FirstName,
                    LastName = registration.LastName,
                    Email = registration.Email,
                    Phone = registration.Phone,
                    Organization = registration.Organization,
                    JobTitle = registration.JobTitle,
                    Status = registration.Status,
                    Type = registration.Type,
                    RegistrationDate = registration.RegistrationDate,
                    CheckInDate = registration.CheckInDate,
                    SpecialRequirements = registration.SpecialRequirements,
                    DietaryRestrictions = registration.DietaryRestrictions,
                    AmountPaid = registration.AmountPaid,
                    PaymentMethod = registration.PaymentMethod,
                    PaymentDate = registration.PaymentDate,
                    Sessions = registration.Sessions.Select(sa => new SessionResponse
                    {
                        Id = sa.Session.Id,
                        Title = sa.Session.Title,
                        Description = sa.Session.Description,
                        StartTime = sa.Session.StartTime,
                        EndTime = sa.Session.EndTime,
                        Location = sa.Session.Location,
                        VirtualMeetingUrl = sa.Session.VirtualMeetingUrl,
                        MaxCapacity = sa.Session.MaxCapacity,
                        Track = sa.Session.Track,
                        Type = sa.Session.Type,
                        Status = sa.Session.Status,
                        Materials = sa.Session.Materials,
                        AttendeeCount = 0
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registration {RegistrationId}", registrationId);
                throw;
            }
        }

        public async Task<List<RegistrationResponse>> GetEventRegistrationsAsync(int eventId)
        {
            try
            {
                var registrations = await _context.Registrations
                    .Include(r => r.Sessions)
                    .ThenInclude(sa => sa.Session)
                    .Where(r => r.EventId == eventId)
                    .OrderBy(r => r.RegistrationDate)
                    .ToListAsync();

                return registrations.Select(r => new RegistrationResponse
                {
                    Id = r.Id,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Email = r.Email,
                    Phone = r.Phone,
                    Organization = r.Organization,
                    JobTitle = r.JobTitle,
                    Status = r.Status,
                    Type = r.Type,
                    RegistrationDate = r.RegistrationDate,
                    CheckInDate = r.CheckInDate,
                    SpecialRequirements = r.SpecialRequirements,
                    DietaryRestrictions = r.DietaryRestrictions,
                    AmountPaid = r.AmountPaid,
                    PaymentMethod = r.PaymentMethod,
                    PaymentDate = r.PaymentDate,
                    Sessions = r.Sessions.Select(sa => new SessionResponse
                    {
                        Id = sa.Session.Id,
                        Title = sa.Session.Title,
                        Description = sa.Session.Description,
                        StartTime = sa.Session.StartTime,
                        EndTime = sa.Session.EndTime,
                        Location = sa.Session.Location,
                        VirtualMeetingUrl = sa.Session.VirtualMeetingUrl,
                        MaxCapacity = sa.Session.MaxCapacity,
                        Track = sa.Session.Track,
                        Type = sa.Session.Type,
                        Status = sa.Session.Status,
                        Materials = sa.Session.Materials,
                        AttendeeCount = 0
                    }).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registrations for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<bool> CheckInAttendeeAsync(int registrationId, int sessionId)
        {
            try
            {
                var sessionAttendee = await _context.SessionAttendees
                    .FirstOrDefaultAsync(sa => sa.RegistrationId == registrationId && sa.SessionId == sessionId);

                if (sessionAttendee == null)
                {
                    // Create new session attendee
                    sessionAttendee = new SessionAttendee
                    {
                        RegistrationId = registrationId,
                        SessionId = sessionId,
                        CheckInTime = DateTime.UtcNow,
                        Attended = true
                    };
                    _context.SessionAttendees.Add(sessionAttendee);
                }
                else
                {
                    sessionAttendee.CheckInTime = DateTime.UtcNow;
                    sessionAttendee.Attended = true;
                }

                // Update registration check-in
                var registration = await _context.Registrations.FindAsync(registrationId);
                if (registration != null)
                {
                    registration.CheckInDate = DateTime.UtcNow;
                    registration.Status = RegistrationStatus.CheckedIn;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Attendee checked in: Registration {RegistrationId} for Session {SessionId}", registrationId, sessionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in attendee");
                throw;
            }
        }

        public async Task<RegistrationResponse> UpdateRegistrationAsync(int registrationId, CreateRegistrationRequest request)
        {
            try
            {
                var registration = await _context.Registrations.FindAsync(registrationId);
                if (registration == null)
                    throw new ArgumentException("Registration not found");

                registration.FirstName = request.FirstName;
                registration.LastName = request.LastName;
                registration.Email = request.Email;
                registration.Phone = request.Phone;
                registration.Organization = request.Organization;
                registration.JobTitle = request.JobTitle;
                registration.Type = request.Type;
                registration.SpecialRequirements = request.SpecialRequirements;
                registration.DietaryRestrictions = request.DietaryRestrictions;
                registration.EmailNotifications = request.EmailNotifications;
                registration.SmsNotifications = request.SmsNotifications;
                registration.SmsPhone = request.SmsPhone;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Registration updated: {RegistrationId}", registrationId);
                return await GetRegistrationAsync(registrationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating registration {RegistrationId}", registrationId);
                throw;
            }
        }

        public async Task<bool> CancelRegistrationAsync(int registrationId)
        {
            try
            {
                var registration = await _context.Registrations.FindAsync(registrationId);
                if (registration == null)
                    return false;

                registration.Status = RegistrationStatus.Cancelled;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Registration cancelled: {RegistrationId}", registrationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling registration {RegistrationId}", registrationId);
                throw;
            }
        }

        #endregion

        #region Speaker Management

        public async Task<SpeakerResponse> CreateSpeakerAsync(Speaker speaker, string userId)
        {
            try
            {
                var tenantId = await GetTenantIdFromUserAsync(userId);
                speaker.TenantId = tenantId;

                _context.Speakers.Add(speaker);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Speaker created: {SpeakerId} by user {UserId}", speaker.Id, userId);
                return new SpeakerResponse
                {
                    Id = speaker.Id,
                    FirstName = speaker.FirstName,
                    LastName = speaker.LastName,
                    Title = speaker.Title,
                    Organization = speaker.Organization,
                    Bio = speaker.Bio,
                    Email = speaker.Email,
                    PhotoUrl = speaker.PhotoUrl,
                    LinkedInUrl = speaker.LinkedInUrl,
                    TwitterUrl = speaker.TwitterUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating speaker");
                throw;
            }
        }

        public async Task<List<SpeakerResponse>> GetSpeakersAsync(int tenantId)
        {
            try
            {
                var speakers = await _context.Speakers
                    .Where(s => s.TenantId == tenantId)
                    .OrderBy(s => s.LastName)
                    .ThenBy(s => s.FirstName)
                    .ToListAsync();

                return speakers.Select(s => new SpeakerResponse
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Title = s.Title,
                    Organization = s.Organization,
                    Bio = s.Bio,
                    Email = s.Email,
                    PhotoUrl = s.PhotoUrl,
                    LinkedInUrl = s.LinkedInUrl,
                    TwitterUrl = s.TwitterUrl
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting speakers for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<SpeakerResponse> GetSpeakerAsync(int speakerId)
        {
            try
            {
                var speaker = await _context.Speakers.FindAsync(speakerId);
                if (speaker == null)
                    throw new ArgumentException("Speaker not found");

                return new SpeakerResponse
                {
                    Id = speaker.Id,
                    FirstName = speaker.FirstName,
                    LastName = speaker.LastName,
                    Title = speaker.Title,
                    Organization = speaker.Organization,
                    Bio = speaker.Bio,
                    Email = speaker.Email,
                    PhotoUrl = speaker.PhotoUrl,
                    LinkedInUrl = speaker.LinkedInUrl,
                    TwitterUrl = speaker.TwitterUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting speaker {SpeakerId}", speakerId);
                throw;
            }
        }

        public async Task<SpeakerResponse> UpdateSpeakerAsync(int speakerId, Speaker speaker, string userId)
        {
            try
            {
                var existingSpeaker = await _context.Speakers.FindAsync(speakerId);
                if (existingSpeaker == null)
                    throw new ArgumentException("Speaker not found");

                existingSpeaker.FirstName = speaker.FirstName;
                existingSpeaker.LastName = speaker.LastName;
                existingSpeaker.Title = speaker.Title;
                existingSpeaker.Organization = speaker.Organization;
                existingSpeaker.Bio = speaker.Bio;
                existingSpeaker.Email = speaker.Email;
                existingSpeaker.Phone = speaker.Phone;
                existingSpeaker.PhotoUrl = speaker.PhotoUrl;
                existingSpeaker.LinkedInUrl = speaker.LinkedInUrl;
                existingSpeaker.TwitterUrl = speaker.TwitterUrl;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Speaker updated: {SpeakerId} by user {UserId}", speakerId, userId);
                return new SpeakerResponse
                {
                    Id = existingSpeaker.Id,
                    FirstName = existingSpeaker.FirstName,
                    LastName = existingSpeaker.LastName,
                    Title = existingSpeaker.Title,
                    Organization = existingSpeaker.Organization,
                    Bio = existingSpeaker.Bio,
                    Email = existingSpeaker.Email,
                    PhotoUrl = existingSpeaker.PhotoUrl,
                    LinkedInUrl = existingSpeaker.LinkedInUrl,
                    TwitterUrl = existingSpeaker.TwitterUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating speaker {SpeakerId}", speakerId);
                throw;
            }
        }

        public async Task<bool> DeleteSpeakerAsync(int speakerId, string userId)
        {
            try
            {
                var speaker = await _context.Speakers.FindAsync(speakerId);
                if (speaker == null)
                    return false;

                _context.Speakers.Remove(speaker);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Speaker deleted: {SpeakerId} by user {UserId}", speakerId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting speaker {SpeakerId}", speakerId);
                throw;
            }
        }

        #endregion

        #region Speaker Assignment

        public async Task<bool> AddSpeakerToEventAsync(int eventId, int speakerId, string role, bool isKeynote = false)
        {
            try
            {
                var eventSpeaker = new EventSpeaker
                {
                    EventId = eventId,
                    SpeakerId = speakerId,
                    Role = role,
                    IsKeynote = isKeynote
                };

                _context.EventSpeakers.Add(eventSpeaker);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Speaker {SpeakerId} added to event {EventId}", speakerId, eventId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding speaker to event");
                throw;
            }
        }

        public async Task<bool> AddSpeakerToSessionAsync(int sessionId, int speakerId, string role)
        {
            try
            {
                var sessionSpeaker = new SessionSpeaker
                {
                    SessionId = sessionId,
                    SpeakerId = speakerId,
                    Role = role
                };

                _context.SessionSpeakers.Add(sessionSpeaker);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Speaker {SpeakerId} added to session {SessionId}", speakerId, sessionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding speaker to session");
                throw;
            }
        }

        #endregion

        #region Analytics

        public async Task<EventAnalytics> GetEventAnalyticsAsync(int eventId)
        {
            try
            {
                var eventData = await _context.Events
                    .Include(e => e.Registrations)
                    .Include(e => e.Sessions)
                    .ThenInclude(s => s.Attendees)
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (eventData == null)
                    throw new ArgumentException("Event not found");

                var totalRegistrations = eventData.Registrations.Count;
                var confirmedRegistrations = eventData.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);
                var checkedInRegistrations = eventData.Registrations.Count(r => r.Status == RegistrationStatus.CheckedIn);
                var totalSessions = eventData.Sessions.Count;
                var totalAttendees = eventData.Sessions.Sum(s => s.Attendees.Count);

                return new EventAnalytics
                {
                    EventId = eventId,
                    TotalRegistrations = totalRegistrations,
                    ConfirmedRegistrations = confirmedRegistrations,
                    CheckedInRegistrations = checkedInRegistrations,
                    TotalSessions = totalSessions,
                    TotalAttendees = totalAttendees,
                    RegistrationRate = totalRegistrations > 0 ? (double)confirmedRegistrations / totalRegistrations * 100 : 0,
                    CheckInRate = confirmedRegistrations > 0 ? (double)checkedInRegistrations / confirmedRegistrations * 100 : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event analytics for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<RegistrationAnalytics> GetRegistrationAnalyticsAsync(int eventId)
        {
            try
            {
                var registrations = await _context.Registrations
                    .Where(r => r.EventId == eventId)
                    .ToListAsync();

                var analytics = new RegistrationAnalytics
                {
                    EventId = eventId,
                    StatusBreakdown = registrations.GroupBy(r => r.Status)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    TypeBreakdown = registrations.GroupBy(r => r.Type)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    DailyRegistrations = registrations.GroupBy(r => r.RegistrationDate.Date)
                        .Select(g => new DailyRegistration
                        {
                            Date = g.Key,
                            Count = g.Count()
                        }).OrderBy(d => d.Date).ToList()
                };

                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registration analytics for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<bool> RemoveSpeakerFromEventAsync(int eventId, int speakerId)
        {
            try
            {
                var eventSpeaker = await _context.EventSpeakers
                    .FirstOrDefaultAsync(es => es.EventId == eventId && es.SpeakerId == speakerId);

                if (eventSpeaker == null)
                    return false;

                _context.EventSpeakers.Remove(eventSpeaker);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Speaker {SpeakerId} removed from event {EventId}", speakerId, eventId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing speaker from event");
                throw;
            }
        }

        public async Task<bool> RemoveSpeakerFromSessionAsync(int sessionId, int speakerId)
        {
            try
            {
                var sessionSpeaker = await _context.SessionSpeakers
                    .FirstOrDefaultAsync(ss => ss.SessionId == sessionId && ss.SpeakerId == speakerId);

                if (sessionSpeaker == null)
                    return false;

                _context.SessionSpeakers.Remove(sessionSpeaker);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Speaker {SpeakerId} removed from session {SessionId}", speakerId, sessionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing speaker from session");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private async Task<int> GetTenantIdFromUserAsync(string userId)
        {
            // This would typically get the tenant ID from the user's context
            // For now, return a default tenant ID
            return 1;
        }

        #endregion
    }

    #region Analytics Models

    public class EventAnalytics
    {
        public int EventId { get; set; }
        public int TotalRegistrations { get; set; }
        public int ConfirmedRegistrations { get; set; }
        public int CheckedInRegistrations { get; set; }
        public int TotalSessions { get; set; }
        public int TotalAttendees { get; set; }
        public double RegistrationRate { get; set; }
        public double CheckInRate { get; set; }
    }

    public class RegistrationAnalytics
    {
        public int EventId { get; set; }
        public Dictionary<RegistrationStatus, int> StatusBreakdown { get; set; } = new();
        public Dictionary<RegistrationType, int> TypeBreakdown { get; set; } = new();
        public List<DailyRegistration> DailyRegistrations { get; set; } = new();
    }

    public class DailyRegistration
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    #endregion
} 