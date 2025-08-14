using Calender.DTO;
using Calender.Models;
using Calender.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly CalendarRepo _calendarRepo;

        public CalendarController(DatabaseContext context)
        {
            _context = context;
            _calendarRepo = new CalendarRepo(context);
        }

        /// <summary>
        /// Get all calendars the current user has access to
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalendarDTO>>> GetUserCalendars()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var calendarIds = await _context.CalendarUsers
                    .Where(cu => cu.UserId == userId)
                    .Select(cu => cu.CalendarId)
                    .ToListAsync();

                var calendars = await _context.Calendars
                    .Where(c => calendarIds.Contains(c.CalendarId))
                    .Include(c => c.CalendarUsers)
                    .ToListAsync();

                var calendarDTOs = calendars.Select(c => new CalendarDTO
                {
                    CalendarId = c.CalendarId,
                    CalendarName = c.CalendarName,
                    UserId = c.Userid,
                    UserPermission = c.CalendarUsers?.FirstOrDefault(cu => cu.UserId == userId)?.Permissions ?? CalendarUser.PermissionLevel.User,
                }).ToArray();

                return Ok(calendarDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific calendar by ID
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<CalendarDTO>> GetCalendar(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var calendar = await _calendarRepo.GetCalendarByIdAsync(id);
                if (calendar == null)
                    return NotFound($"Calendar with ID {id} not found.");

                // Check permissions
                if (!await HasCalendarAccess(id, userId.Value))
                    return Forbid("You don't have access to this calendar.");

                var userPermission = await _context.CalendarUsers
                    .Where(cu => cu.CalendarId == id && cu.UserId == userId)
                    .Select(cu => cu.Permissions)
                    .FirstOrDefaultAsync();

                var calendarDTO = new CalendarDTO
                {
                    CalendarId = calendar.CalendarId,
                    CalendarName = calendar.CalendarName,
                    UserId = calendar.Userid,
                    UserPermission = userPermission,
                };

                return Ok(calendarDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new calendar
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CalendarDTO>> CreateCalendar([FromBody] CreateCalendarRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                if (string.IsNullOrWhiteSpace(request.CalendarName))
                    return BadRequest("CalendarName is required.");

                var calendar = new Calendar 
                { 
                    CalendarName = request.CalendarName.Trim(), 
                    Userid = userId.Value 
                };

                int calendarId = await _calendarRepo.AddCalendarAsync(calendar);

                // Add creator as Owner
                var ownerLink = new CalendarUser
                {
                    CalendarId = calendarId,
                    UserId = userId.Value,
                    Permissions = CalendarUser.PermissionLevel.Owner
                };

                _context.CalendarUsers.Add(ownerLink);
                await _context.SaveChangesAsync();

                var calendarDTO = new CalendarDTO
                {
                    CalendarId = calendarId,
                    CalendarName = request.CalendarName.Trim(),
                    UserId = userId.Value,
                    UserPermission = CalendarUser.PermissionLevel.Owner,
                };

                return CreatedAtAction(nameof(GetCalendar), new { id = calendarId }, calendarDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Add a user to a calendar with specified permissions
        /// </summary>
        [Authorize]
        [HttpPost("{calendarId}/users")]
        public async Task<IActionResult> AddUserToCalendar(int calendarId, [FromBody] AddUserRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null) return Unauthorized();

                // Check if current user has permission to add users
                if (!await HasModeratorAccess(calendarId, currentUserId.Value))
                    return Forbid("You don't have permission to add users to this calendar.");

                // Check if target user exists
                var targetUserExists = await _context.Users.AnyAsync(u => u.UserId == request.UserId);
                if (!targetUserExists)
                    return BadRequest("Target user does not exist.");

                // Check if user is already in calendar
                var exists = await _context.CalendarUsers
                    .AnyAsync(cu => cu.CalendarId == calendarId && cu.UserId == request.UserId);

                if (exists)
                    return BadRequest("User is already a member of this calendar.");

                var newEntry = new CalendarUser
                {
                    CalendarId = calendarId,
                    UserId = request.UserId,
                    Permissions = request.Permission
                };

                _context.CalendarUsers.Add(newEntry);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update calendar details
        /// </summary>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCalendar(int id, [FromBody] UpdateCalendarRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                if (string.IsNullOrWhiteSpace(request.CalendarName))
                    return BadRequest("CalendarName is required.");

                if (!await HasModeratorAccess(id, userId.Value))
                    return Forbid("You don't have permission to update this calendar.");

                var calendar = new Calendar
                {
                    CalendarId = id,
                    CalendarName = request.CalendarName.Trim(),
                    Userid = userId.Value // Keep current user as owner
                };

                await _calendarRepo.UpdateCalendarAsync(calendar);
                return Ok(new { message = "Calendar updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a calendar
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCalendar(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                if (!await HasOwnerAccess(id, userId.Value))
                    return Forbid("Only the owner can delete this calendar.");

                await _calendarRepo.DeleteCalendarAsync(id);
                return Ok(new { message = "Calendar deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Calendar not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all events for a specific calendar
        /// </summary>
        [Authorize]
        [HttpGet("{id}/events")]
        public async Task<ActionResult<IEnumerable<Event>>> GetCalendarEvents(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                if (!await HasCalendarAccess(id, userId.Value))
                    return Forbid("You don't have access to this calendar's events.");

                var events = await _context.Events
                    .Where(e => e.CalendarId == id)
                    .OrderBy(e => e.EventStart)
                    .ToListAsync();

                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get events for a specific date range
        /// </summary>
        [Authorize]
        [HttpGet("{id}/events/range")]
        public async Task<ActionResult<IEnumerable<Event>>> GetCalendarEventsByDateRange(
            int id, 
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                if (!await HasCalendarAccess(id, userId.Value))
                    return Forbid("You don't have access to this calendar's events.");

                var events = await _context.Events
                    .Where(e => e.CalendarId == id && 
                               e.EventStart >= startDate && 
                               e.EventStart <= endDate)
                    .OrderBy(e => e.EventStart)
                    .ToListAsync();

                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get calendar members
        /// </summary>
        [Authorize]
        [HttpGet("{id}/members")]
        public async Task<ActionResult<IEnumerable<CalendarMemberDTO>>> GetCalendarMembers(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                if (!await HasCalendarAccess(id, userId.Value))
                    return Forbid("You don't have access to this calendar.");

                var members = await _context.CalendarUsers
                    .Where(cu => cu.CalendarId == id)
                    .Include(cu => cu.User)
                    .Select(cu => new CalendarMemberDTO
                    {
                        UserId = cu.UserId,
                        Username = cu.User!.Username,
                        Permission = cu.Permissions
                    })
                    .ToListAsync();

                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper methods
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }

        private bool IsWebsiteAdmin()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value == "WebsiteAdmin";
        }

        private async Task<bool> HasCalendarAccess(int calendarId, int userId)
        {
            if (IsWebsiteAdmin()) return true;

            return await _context.CalendarUsers
                .AnyAsync(cu => cu.CalendarId == calendarId && cu.UserId == userId);
        }

        private async Task<bool> HasModeratorAccess(int calendarId, int userId)
        {
            if (IsWebsiteAdmin()) return true;

            var permission = await _context.CalendarUsers
                .Where(cu => cu.CalendarId == calendarId && cu.UserId == userId)
                .Select(cu => cu.Permissions)
                .FirstOrDefaultAsync();

            return permission == CalendarUser.PermissionLevel.Owner || 
                   permission == CalendarUser.PermissionLevel.Moderator;
        }

        private async Task<bool> HasOwnerAccess(int calendarId, int userId)
        {
            if (IsWebsiteAdmin()) return true;

            var permission = await _context.CalendarUsers
                .Where(cu => cu.CalendarId == calendarId && cu.UserId == userId)
                .Select(cu => cu.Permissions)
                .FirstOrDefaultAsync();

            return permission == CalendarUser.PermissionLevel.Owner;
        }
    }

    // Request/Response DTOs
    public class CreateCalendarRequest
    {
        public string CalendarName { get; set; } = string.Empty;
    }

    public class UpdateCalendarRequest
    {
        public string CalendarName { get; set; } = string.Empty;
    }

    public class AddUserRequest
    {
        public int UserId { get; set; }
        public CalendarUser.PermissionLevel Permission { get; set; }
    }

    public class CalendarMemberDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public CalendarUser.PermissionLevel Permission { get; set; }
    }
}