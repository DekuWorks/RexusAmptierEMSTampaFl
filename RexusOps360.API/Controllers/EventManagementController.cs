using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RexusOps360.API.Models;
using RexusOps360.API.Services;
using System.Security.Claims;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventManagementController : ControllerBase
    {
        private readonly IEventManagementService _eventService;
        private readonly ILogger<EventManagementController> _logger;

        public EventManagementController(IEventManagementService eventService, ILogger<EventManagementController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        #region Event Management

        /// <summary>
        /// Create a new event
        /// </summary>
        [HttpPost("events")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult<EventResponse>> CreateEvent([FromBody] CreateEventRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var eventResponse = await _eventService.CreateEventAsync(request, userId);
                return CreatedAtAction(nameof(GetEvent), new { eventId = eventResponse.Id }, eventResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating event");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                return StatusCode(500, new { error = "An error occurred while creating the event" });
            }
        }

        /// <summary>
        /// Get a specific event by ID
        /// </summary>
        [HttpGet("events/{eventId}")]
        public async Task<ActionResult<EventResponse>> GetEvent(int eventId)
        {
            try
            {
                var eventResponse = await _eventService.GetEventAsync(eventId);
                return Ok(eventResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Event not found: {EventId}", eventId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while retrieving the event" });
            }
        }

        /// <summary>
        /// Get all events for the current tenant
        /// </summary>
        [HttpGet("events")]
        public async Task<ActionResult<List<EventResponse>>> GetEvents([FromQuery] bool includeDrafts = false)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                // Get tenant ID from user context (simplified for demo)
                var tenantId = 1; // This would come from user context
                var events = await _eventService.GetEventsAsync(tenantId, includeDrafts);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events");
                return StatusCode(500, new { error = "An error occurred while retrieving events" });
            }
        }

        /// <summary>
        /// Update an existing event
        /// </summary>
        [HttpPut("events/{eventId}")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult<EventResponse>> UpdateEvent(int eventId, [FromBody] UpdateEventRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var eventResponse = await _eventService.UpdateEventAsync(eventId, request, userId);
                return Ok(eventResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Event not found or invalid request: {EventId}", eventId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while updating the event" });
            }
        }

        /// <summary>
        /// Delete an event
        /// </summary>
        [HttpDelete("events/{eventId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteEvent(int eventId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var success = await _eventService.DeleteEventAsync(eventId, userId);
                if (!success)
                    return NotFound(new { error = "Event not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while deleting the event" });
            }
        }

        #endregion

        #region Session Management

        /// <summary>
        /// Create a new session for an event
        /// </summary>
        [HttpPost("sessions")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult<SessionResponse>> CreateSession([FromBody] CreateSessionRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var sessionResponse = await _eventService.CreateSessionAsync(request, userId);
                return CreatedAtAction(nameof(GetSession), new { sessionId = sessionResponse.Id }, sessionResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating session");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session");
                return StatusCode(500, new { error = "An error occurred while creating the session" });
            }
        }

        /// <summary>
        /// Get a specific session by ID
        /// </summary>
        [HttpGet("sessions/{sessionId}")]
        public async Task<ActionResult<SessionResponse>> GetSession(int sessionId)
        {
            try
            {
                var sessionResponse = await _eventService.GetSessionAsync(sessionId);
                return Ok(sessionResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Session not found: {SessionId}", sessionId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session {SessionId}", sessionId);
                return StatusCode(500, new { error = "An error occurred while retrieving the session" });
            }
        }

        /// <summary>
        /// Get all sessions for an event
        /// </summary>
        [HttpGet("events/{eventId}/sessions")]
        public async Task<ActionResult<List<SessionResponse>>> GetEventSessions(int eventId)
        {
            try
            {
                var sessions = await _eventService.GetEventSessionsAsync(eventId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sessions for event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while retrieving sessions" });
            }
        }

        /// <summary>
        /// Update an existing session
        /// </summary>
        [HttpPut("sessions/{sessionId}")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult<SessionResponse>> UpdateSession(int sessionId, [FromBody] CreateSessionRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var sessionResponse = await _eventService.UpdateSessionAsync(sessionId, request, userId);
                return Ok(sessionResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Session not found or invalid request: {SessionId}", sessionId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session {SessionId}", sessionId);
                return StatusCode(500, new { error = "An error occurred while updating the session" });
            }
        }

        /// <summary>
        /// Delete a session
        /// </summary>
        [HttpDelete("sessions/{sessionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteSession(int sessionId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var success = await _eventService.DeleteSessionAsync(sessionId, userId);
                if (!success)
                    return NotFound(new { error = "Session not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
                return StatusCode(500, new { error = "An error occurred while deleting the session" });
            }
        }

        #endregion

        #region Registration Management

        /// <summary>
        /// Register for an event
        /// </summary>
        [HttpPost("registrations")]
        [AllowAnonymous] // Allow public registration
        public async Task<ActionResult<RegistrationResponse>> CreateRegistration([FromBody] CreateRegistrationRequest request)
        {
            try
            {
                var registrationResponse = await _eventService.CreateRegistrationAsync(request);
                return CreatedAtAction(nameof(GetRegistration), new { registrationId = registrationResponse.Id }, registrationResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating registration");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating registration");
                return StatusCode(500, new { error = "An error occurred while creating the registration" });
            }
        }

        /// <summary>
        /// Get a specific registration by ID
        /// </summary>
        [HttpGet("registrations/{registrationId}")]
        public async Task<ActionResult<RegistrationResponse>> GetRegistration(int registrationId)
        {
            try
            {
                var registrationResponse = await _eventService.GetRegistrationAsync(registrationId);
                return Ok(registrationResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Registration not found: {RegistrationId}", registrationId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registration {RegistrationId}", registrationId);
                return StatusCode(500, new { error = "An error occurred while retrieving the registration" });
            }
        }

        /// <summary>
        /// Get all registrations for an event
        /// </summary>
        [HttpGet("events/{eventId}/registrations")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult<List<RegistrationResponse>>> GetEventRegistrations(int eventId)
        {
            try
            {
                var registrations = await _eventService.GetEventRegistrationsAsync(eventId);
                return Ok(registrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registrations for event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while retrieving registrations" });
            }
        }

        /// <summary>
        /// Check in an attendee for a session
        /// </summary>
        [HttpPost("registrations/{registrationId}/checkin")]
        [Authorize(Roles = "Admin,EventManager,CheckInStaff")]
        public async Task<ActionResult> CheckInAttendee(int registrationId, [FromQuery] int sessionId)
        {
            try
            {
                var success = await _eventService.CheckInAttendeeAsync(registrationId, sessionId);
                if (!success)
                    return BadRequest(new { error = "Unable to check in attendee" });

                return Ok(new { message = "Attendee checked in successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in attendee {RegistrationId} for session {SessionId}", registrationId, sessionId);
                return StatusCode(500, new { error = "An error occurred while checking in the attendee" });
            }
        }

        /// <summary>
        /// Cancel a registration
        /// </summary>
        [HttpPost("registrations/{registrationId}/cancel")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult> CancelRegistration(int registrationId)
        {
            try
            {
                var success = await _eventService.CancelRegistrationAsync(registrationId);
                if (!success)
                    return NotFound(new { error = "Registration not found" });

                return Ok(new { message = "Registration cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling registration {RegistrationId}", registrationId);
                return StatusCode(500, new { error = "An error occurred while cancelling the registration" });
            }
        }

        #endregion

        #region Speaker Management

        /// <summary>
        /// Create a new speaker
        /// </summary>
        [HttpPost("speakers")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult<SpeakerResponse>> CreateSpeaker([FromBody] Speaker speaker)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var speakerResponse = await _eventService.CreateSpeakerAsync(speaker, userId);
                return CreatedAtAction(nameof(GetSpeaker), new { speakerId = speakerResponse.Id }, speakerResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating speaker");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating speaker");
                return StatusCode(500, new { error = "An error occurred while creating the speaker" });
            }
        }

        /// <summary>
        /// Get a specific speaker by ID
        /// </summary>
        [HttpGet("speakers/{speakerId}")]
        public async Task<ActionResult<SpeakerResponse>> GetSpeaker(int speakerId)
        {
            try
            {
                var speakerResponse = await _eventService.GetSpeakerAsync(speakerId);
                return Ok(speakerResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Speaker not found: {SpeakerId}", speakerId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting speaker {SpeakerId}", speakerId);
                return StatusCode(500, new { error = "An error occurred while retrieving the speaker" });
            }
        }

        /// <summary>
        /// Get all speakers for the current tenant
        /// </summary>
        [HttpGet("speakers")]
        public async Task<ActionResult<List<SpeakerResponse>>> GetSpeakers()
        {
            try
            {
                // Get tenant ID from user context (simplified for demo)
                var tenantId = 1; // This would come from user context
                var speakers = await _eventService.GetSpeakersAsync(tenantId);
                return Ok(speakers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting speakers");
                return StatusCode(500, new { error = "An error occurred while retrieving speakers" });
            }
        }

        /// <summary>
        /// Update an existing speaker
        /// </summary>
        [HttpPut("speakers/{speakerId}")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult<SpeakerResponse>> UpdateSpeaker(int speakerId, [FromBody] Speaker speaker)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var speakerResponse = await _eventService.UpdateSpeakerAsync(speakerId, speaker, userId);
                return Ok(speakerResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Speaker not found or invalid request: {SpeakerId}", speakerId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating speaker {SpeakerId}", speakerId);
                return StatusCode(500, new { error = "An error occurred while updating the speaker" });
            }
        }

        /// <summary>
        /// Delete a speaker
        /// </summary>
        [HttpDelete("speakers/{speakerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteSpeaker(int speakerId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var success = await _eventService.DeleteSpeakerAsync(speakerId, userId);
                if (!success)
                    return NotFound(new { error = "Speaker not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting speaker {SpeakerId}", speakerId);
                return StatusCode(500, new { error = "An error occurred while deleting the speaker" });
            }
        }

        #endregion

        #region Speaker Assignment

        /// <summary>
        /// Add a speaker to an event
        /// </summary>
        [HttpPost("events/{eventId}/speakers/{speakerId}")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult> AddSpeakerToEvent(int eventId, int speakerId, [FromQuery] string role = "Speaker", [FromQuery] bool isKeynote = false)
        {
            try
            {
                var success = await _eventService.AddSpeakerToEventAsync(eventId, speakerId, role, isKeynote);
                if (!success)
                    return BadRequest(new { error = "Unable to add speaker to event" });

                return Ok(new { message = "Speaker added to event successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding speaker {SpeakerId} to event {EventId}", speakerId, eventId);
                return StatusCode(500, new { error = "An error occurred while adding the speaker to the event" });
            }
        }

        /// <summary>
        /// Remove a speaker from an event
        /// </summary>
        [HttpDelete("events/{eventId}/speakers/{speakerId}")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult> RemoveSpeakerFromEvent(int eventId, int speakerId)
        {
            try
            {
                var success = await _eventService.RemoveSpeakerFromEventAsync(eventId, speakerId);
                if (!success)
                    return NotFound(new { error = "Speaker not found in event" });

                return Ok(new { message = "Speaker removed from event successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing speaker {SpeakerId} from event {EventId}", speakerId, eventId);
                return StatusCode(500, new { error = "An error occurred while removing the speaker from the event" });
            }
        }

        /// <summary>
        /// Add a speaker to a session
        /// </summary>
        [HttpPost("sessions/{sessionId}/speakers/{speakerId}")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult> AddSpeakerToSession(int sessionId, int speakerId, [FromQuery] string role = "Speaker")
        {
            try
            {
                var success = await _eventService.AddSpeakerToSessionAsync(sessionId, speakerId, role);
                if (!success)
                    return BadRequest(new { error = "Unable to add speaker to session" });

                return Ok(new { message = "Speaker added to session successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding speaker {SpeakerId} to session {SessionId}", speakerId, sessionId);
                return StatusCode(500, new { error = "An error occurred while adding the speaker to the session" });
            }
        }

        /// <summary>
        /// Remove a speaker from a session
        /// </summary>
        [HttpDelete("sessions/{sessionId}/speakers/{speakerId}")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult> RemoveSpeakerFromSession(int sessionId, int speakerId)
        {
            try
            {
                var success = await _eventService.RemoveSpeakerFromSessionAsync(sessionId, speakerId);
                if (!success)
                    return NotFound(new { error = "Speaker not found in session" });

                return Ok(new { message = "Speaker removed from session successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing speaker {SpeakerId} from session {SessionId}", speakerId, sessionId);
                return StatusCode(500, new { error = "An error occurred while removing the speaker from the session" });
            }
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Get analytics for an event
        /// </summary>
        [HttpGet("events/{eventId}/analytics")]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<ActionResult<EventAnalytics>> GetEventAnalytics(int eventId)
        {
            try
            {
                var analytics = await _eventService.GetEventAnalyticsAsync(eventId);
                return Ok(analytics);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Event not found for analytics: {EventId}", eventId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics for event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while retrieving analytics" });
            }
        }

        #endregion
    }
} 