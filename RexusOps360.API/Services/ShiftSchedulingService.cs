using RexusOps360.API.Models;
using RexusOps360.API.Data;

namespace RexusOps360.API.Services
{
    public interface IShiftSchedulingService
    {
        Task<ShiftSchedule> CreateShiftAsync(CreateShiftRequest request);
        Task<ShiftSchedule?> GetShiftAsync(int shiftId);
        Task<List<ShiftSchedule>> GetShiftsByResponderAsync(string responderId);
        Task<List<ShiftSchedule>> GetShiftsByDateAsync(DateTime date);
        Task<List<ShiftSchedule>> GetUpcomingShiftsAsync();
        Task<ShiftSchedule> UpdateShiftAsync(int shiftId, UpdateShiftRequest request);
        Task<bool> DeleteShiftAsync(int shiftId);
        Task<List<ResponderAvailability>> GetAvailableRespondersAsync(DateTime startTime, DateTime endTime);
    }

    public class ShiftSchedule
    {
        public int Id { get; set; }
        public string ResponderId { get; set; } = string.Empty;
        public string ResponderName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ShiftType { get; set; } = string.Empty; // "Day", "Night", "Overtime"
        public string Status { get; set; } = "Scheduled"; // "Scheduled", "In Progress", "Completed", "Cancelled"
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateShiftRequest
    {
        public string ResponderId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ShiftType { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class UpdateShiftRequest
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? ShiftType { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }

    public class ResponderAvailability
    {
        public string ResponderId { get; set; } = string.Empty;
        public string ResponderName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public List<ShiftSchedule> ConflictingShifts { get; set; } = new();
    }

    public class ShiftSchedulingService : IShiftSchedulingService
    {
        private static readonly List<ShiftSchedule> _shifts = new();
        private static int _nextShiftId = 1;

        public Task<ShiftSchedule> CreateShiftAsync(CreateShiftRequest request)
        {
            var shift = new ShiftSchedule
            {
                Id = _nextShiftId++,
                ResponderId = request.ResponderId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                ShiftType = request.ShiftType,
                Notes = request.Notes,
                Status = "Scheduled"
            };

            _shifts.Add(shift);
            return Task.FromResult(shift);
        }

        public Task<ShiftSchedule?> GetShiftAsync(int shiftId)
        {
            var shift = _shifts.FirstOrDefault(s => s.Id == shiftId);
            return Task.FromResult(shift);
        }

        public Task<List<ShiftSchedule>> GetShiftsByResponderAsync(string responderId)
        {
            var shifts = _shifts.Where(s => s.ResponderId == responderId)
                               .OrderBy(s => s.StartTime)
                               .ToList();
            return Task.FromResult(shifts);
        }

        public Task<List<ShiftSchedule>> GetShiftsByDateAsync(DateTime date)
        {
            var shifts = _shifts.Where(s => s.StartTime.Date == date.Date)
                               .OrderBy(s => s.StartTime)
                               .ToList();
            return Task.FromResult(shifts);
        }

        public Task<List<ShiftSchedule>> GetUpcomingShiftsAsync()
        {
            var now = DateTime.UtcNow;
            var shifts = _shifts.Where(s => s.StartTime > now && s.Status == "Scheduled")
                               .OrderBy(s => s.StartTime)
                               .ToList();
            return Task.FromResult(shifts);
        }

        public Task<ShiftSchedule> UpdateShiftAsync(int shiftId, UpdateShiftRequest request)
        {
            var shift = _shifts.FirstOrDefault(s => s.Id == shiftId);
            if (shift == null)
                throw new ArgumentException("Shift not found");

            if (request.StartTime.HasValue)
                shift.StartTime = request.StartTime.Value;
            if (request.EndTime.HasValue)
                shift.EndTime = request.EndTime.Value;
            if (!string.IsNullOrEmpty(request.ShiftType))
                shift.ShiftType = request.ShiftType;
            if (!string.IsNullOrEmpty(request.Status))
                shift.Status = request.Status;
            if (request.Notes != null)
                shift.Notes = request.Notes;

            shift.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(shift);
        }

        public Task<bool> DeleteShiftAsync(int shiftId)
        {
            var shift = _shifts.FirstOrDefault(s => s.Id == shiftId);
            if (shift == null)
                return Task.FromResult(false);

            var result = _shifts.Remove(shift);
            return Task.FromResult(result);
        }

        public Task<List<ResponderAvailability>> GetAvailableRespondersAsync(DateTime startTime, DateTime endTime)
        {
            var responders = InMemoryStore.GetAllResponders();
            var availabilities = new List<ResponderAvailability>();

            foreach (var responder in responders)
            {
                var conflictingShifts = _shifts.Where(s => 
                    s.ResponderId == responder.Id.ToString() &&
                    s.Status != "Cancelled" &&
                    ((s.StartTime <= startTime && s.EndTime > startTime) ||
                     (s.StartTime < endTime && s.EndTime >= endTime) ||
                     (s.StartTime >= startTime && s.EndTime <= endTime)))
                    .ToList();

                var availability = new ResponderAvailability
                {
                    ResponderId = responder.Id.ToString(),
                    ResponderName = responder.Name,
                    Role = responder.Role,
                    IsAvailable = !conflictingShifts.Any(),
                    ConflictingShifts = conflictingShifts
                };

                availabilities.Add(availability);
            }

            return Task.FromResult(availabilities);
        }
    }
} 