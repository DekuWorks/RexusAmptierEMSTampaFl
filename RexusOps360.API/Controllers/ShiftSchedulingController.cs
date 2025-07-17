using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Services;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftSchedulingController : ControllerBase
    {
        private readonly IShiftSchedulingService _shiftSchedulingService;

        public ShiftSchedulingController(IShiftSchedulingService shiftSchedulingService)
        {
            _shiftSchedulingService = shiftSchedulingService;
        }

        [HttpPost("shifts")]
        public async Task<IActionResult> CreateShift([FromBody] CreateShiftRequest request)
        {
            try
            {
                var shift = await _shiftSchedulingService.CreateShiftAsync(request);
                return CreatedAtAction(nameof(GetShift), new { shiftId = shift.Id }, shift);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("shifts/{shiftId}")]
        public async Task<IActionResult> GetShift(int shiftId)
        {
            var shift = await _shiftSchedulingService.GetShiftAsync(shiftId);
            if (shift == null)
                return NotFound(new { error = "Shift not found" });

            return Ok(shift);
        }

        [HttpGet("shifts/responder/{responderId}")]
        public async Task<IActionResult> GetShiftsByResponder(string responderId)
        {
            var shifts = await _shiftSchedulingService.GetShiftsByResponderAsync(responderId);
            return Ok(shifts);
        }

        [HttpGet("shifts/date/{date:datetime}")]
        public async Task<IActionResult> GetShiftsByDate(DateTime date)
        {
            var shifts = await _shiftSchedulingService.GetShiftsByDateAsync(date);
            return Ok(shifts);
        }

        [HttpGet("shifts/upcoming")]
        public async Task<IActionResult> GetUpcomingShifts()
        {
            var shifts = await _shiftSchedulingService.GetUpcomingShiftsAsync();
            return Ok(shifts);
        }

        [HttpPut("shifts/{shiftId}")]
        public async Task<IActionResult> UpdateShift(int shiftId, [FromBody] UpdateShiftRequest request)
        {
            try
            {
                var shift = await _shiftSchedulingService.UpdateShiftAsync(shiftId, request);
                return Ok(shift);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("shifts/{shiftId}")]
        public async Task<IActionResult> DeleteShift(int shiftId)
        {
            var deleted = await _shiftSchedulingService.DeleteShiftAsync(shiftId);
            if (!deleted)
                return NotFound(new { error = "Shift not found" });

            return Ok(new { message = "Shift deleted successfully" });
        }

        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailableResponders([FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
        {
            var availabilities = await _shiftSchedulingService.GetAvailableRespondersAsync(startTime, endTime);
            return Ok(availabilities);
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendarView([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var calendarData = new List<object>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var shifts = await _shiftSchedulingService.GetShiftsByDateAsync(currentDate);
                calendarData.Add(new
                {
                    Date = currentDate,
                    Shifts = shifts
                });
                currentDate = currentDate.AddDays(1);
            }

            return Ok(calendarData);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetShiftStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var allShifts = new List<ShiftSchedule>();
            var currentDate = start.Date;

            while (currentDate <= end.Date)
            {
                var shifts = await _shiftSchedulingService.GetShiftsByDateAsync(currentDate);
                allShifts.AddRange(shifts);
                currentDate = currentDate.AddDays(1);
            }

            var statistics = new
            {
                TotalShifts = allShifts.Count,
                ScheduledShifts = allShifts.Count(s => s.Status == "Scheduled"),
                CompletedShifts = allShifts.Count(s => s.Status == "Completed"),
                CancelledShifts = allShifts.Count(s => s.Status == "Cancelled"),
                DayShifts = allShifts.Count(s => s.ShiftType == "Day"),
                NightShifts = allShifts.Count(s => s.ShiftType == "Night"),
                OvertimeShifts = allShifts.Count(s => s.ShiftType == "Overtime"),
                AverageShiftDuration = allShifts.Any() ? 
                    allShifts.Average(s => (s.EndTime - s.StartTime).TotalHours) : 0
            };

            return Ok(statistics);
        }
    }
} 