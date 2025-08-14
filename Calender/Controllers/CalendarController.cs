using Calender.DTO;
using Calender.Models;
using Calender.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security;
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

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalendarDTO>>> GetCalendar()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var calendarIds = await _context.CalendarUsers
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.CalendarId)
                .ToListAsync();

            var calendars = await _context.Calendars
                .Where(c => calendarIds.Contains(c.CalendarId))
                .Select(c => new Calendar
                {
                    CalendarId = c.CalendarId,
                    CalendarName = c.CalendarName,
                    Userid = c.Userid
                })
                .ToListAsync();

            CalendarDTO[] calendarDTOs = calendars.Select(c => new CalendarDTO
            {
                CalendarId = c.CalendarId,
                CalendarName = c.CalendarName,
                Userid = c.Userid
            }).ToArray();

            return Ok(calendarDTOs);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Calendar>> GetCalendar(int id)
        {
            var calendar = await _calendarRepo.GetCalendarByIdAsync(id);
            if (calendar == null)
                return NotFound($"Calendar with ID {id} not found.");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin")
            {
                var permission = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == id && cu.UserId == userId);

                if (permission == null)
                    return StatusCode(403, "Ingen adgang til denne kalender.");
            }

            return Ok(calendar);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Calendar>> CreateCalendar(String CalendarName)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (CalendarName == null || string.IsNullOrWhiteSpace(CalendarName))
                return BadRequest("CalendarName is required.");        

            int calendarId = await _calendarRepo.AddCalendarAsync(new Calendar { CalendarName = CalendarName, Userid = userId });

            // Tilføj opretter som Owner i CalendarUsers
            var ownerLink = new CalendarUser
            {
                CalendarId = calendarId,
                UserId = userId,
                Permissions = CalendarUser.PermissionLevel.Owner
            };

            _context.CalendarUsers.Add(ownerLink);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCalendar), new { id = calendarId }, new Calendar { CalendarId = calendarId, CalendarName = CalendarName, Userid = userId });
        }

        [Authorize]
        [HttpPost("{calendarId}/users/{userId}")]
        public async Task<IActionResult> AddUserToCalendar(int calendarId, int userId, [FromBody] CalendarUser.PermissionLevel level)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin")
            {
                var perm = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == calendarId && cu.UserId == currentUserId);

                if (perm == null || (perm.Permissions != CalendarUser.PermissionLevel.Owner && perm.Permissions != CalendarUser.PermissionLevel.Moderator))
                    return Forbid("Du har ikke admin-rettigheder til denne kalender.");
            }

            var exists = await _context.CalendarUsers
                .AnyAsync(cu => cu.CalendarId == calendarId && cu.UserId == userId);

            if (exists)
                return BadRequest("Brugeren er allerede tilknyttet kalenderen.");

            var newEntry = new CalendarUser
            {
                CalendarId = calendarId,
                UserId = userId,
                Permissions = level
            };

            _context.CalendarUsers.Add(newEntry);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCalendar(int id, Calendar updateCalendar)
        {
            if (string.IsNullOrWhiteSpace(updateCalendar.CalendarName))
                return BadRequest("CalendarName is required.");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin")
            {
                var perm = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == id && cu.UserId == userId);

                if (perm == null || (perm.Permissions != CalendarUser.PermissionLevel.Owner && perm.Permissions != CalendarUser.PermissionLevel.Moderator))
                    return Forbid("Kun ejeren eller en moderator kan opdatere kalenderen.");
            }
            updateCalendar.CalendarId = id;

            try
            {
                await _calendarRepo.UpdateCalendarAsync(updateCalendar);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCalendar(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var calendar = await _context.Calendars.FindAsync(id);
            if (calendar == null)
                return NotFound("Kalenderen blev ikke fundet.");

            if (role != "WebsiteAdmin")
            {
                var perm = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == id && cu.UserId == userId);

                if (perm == null || (perm.Permissions != CalendarUser.PermissionLevel.Owner && perm.Permissions != CalendarUser.PermissionLevel.Moderator))
                    return StatusCode(403, "Kun ejeren eller en moderator kan slette kalenderen.");

            }

            await _calendarRepo.DeleteCalendarAsync(id); // Repository tager sig af selve sletningen
            return NoContent();
        }


        [Authorize]
        [HttpGet("{id}/events")]
        public async Task<ActionResult<IEnumerable<Event>>> GetCalendarEvents(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin")
            {
                var permission = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == id && cu.UserId == userId);

                if (permission == null)
                    return Forbid("Ingen adgang til denne kalenders events.");
            }

            var events = await _context.Events
                .Where(e => e.CalendarId == id)
                .ToListAsync();

            return Ok(events);
        }

        [Authorize]
        [HttpGet("{id}/invitations")]
        public async Task<ActionResult<IEnumerable<CalendarInvitation>>> GetCalendarInvitations(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin")
            {
                var permission = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == id && cu.UserId == userId);

                if (permission == null || !permission.IsOwner)
                    return Forbid("Kun ejeren kan se invitationerne.");
            }

            var invitations = await _context.CalendarInvitations
                .Where(ci => ci.CalendarId == id)
                .ToListAsync();

            return Ok(invitations);
        }
    }
}
