using Calender.Models;
using Microsoft.AspNetCore.Authorization;
using Calender.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarUserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly CalendarUserRepo _calendarUserRepo;

        public CalendarUserController(DatabaseContext context)
        {
            _context = context;
            _calendarUserRepo = new CalendarUserRepo(context);
        }

        // Hent alle CalendarUsers – kun for WebsiteAdmin
        [Authorize(Roles = "WebsiteAdmin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalendarUser>>> GetAllCUsers()
        {
            var calendarUsers = await _calendarUserRepo.GetAllCalendarUsersAsync();
            return Ok(calendarUsers);
        }

        // Hent en bestemt CalendarUser – WebsiteAdmin, brugeren selv eller ejer
        [Authorize]
        [HttpGet("{calendarId}/{userId}")]
        public async Task<ActionResult<CalendarUser>> GetCalendarUser(int calendarId, int userId)
        {
            var calendarUser = await _calendarUserRepo.GetCalendarUserAsync(calendarId, userId);
            if (calendarUser == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin" &&
                currentUserId != userId &&
                !await IsOwner(calendarId, currentUserId))
                return Forbid();

            return Ok(calendarUser);
        }

        // Opret en ny CalendarUser – kun Owner/Admin
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CalendarUser>> CreateCUser(CalendarUser cuser)
        {
            if (cuser == null) return BadRequest();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin" &&
                !await IsOwner(cuser.CalendarId, currentUserId))
                return Forbid("Only the calendar owner or WebsiteAdmin can add users.");

            // Valider fremmednøgler
            var userExists = await _context.Users.AnyAsync(u => u.UserId == cuser.UserId);
            var calendarExists = await _context.Calendars.AnyAsync(c => c.CalendarId == cuser.CalendarId);

            if (!userExists || !calendarExists)
                return BadRequest("User or Calendar doesn't exist.");

            _context.CalendarUsers.Add(cuser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCalendarUser), new { calendarId = cuser.CalendarId, userId = cuser.UserId }, cuser);
        }

        // Opdater CalendarUser – kun Owner
        [Authorize]
        [HttpPut("{calendarId}/{userId}")]
        public async Task<IActionResult> UpdateCUser(int calendarId, int userId, CalendarUser updatecuser)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin" &&
                !await IsOwner(calendarId, currentUserId))
                return Forbid("Only the owner can change user permissions.");

            var cuser = await _context.CalendarUsers
                .FirstOrDefaultAsync(cu => cu.CalendarId == calendarId && cu.UserId == userId);

            if (cuser == null)
                return NotFound();

            cuser.Permissions = updatecuser.Permissions;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Slet CalendarUser – kun Owner eller brugeren selv
        [Authorize]
        [HttpDelete("{calendarId}/{userId}")]
        public async Task<IActionResult> DeleteCuser(int calendarId, int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin" &&
                currentUserId != userId &&
                !await IsOwner(calendarId, currentUserId))
                return Forbid("Kun ejeren eller brugeren selv kan fjerne tilknytningen.");

            try
            {
                await _calendarUserRepo.DeleteCalendarUserAsync(calendarId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // 🔒 Hjælpemetode til ejertjek
        private async Task<bool> IsOwner(int calendarId, int userId)
        {
            var cu = await _context.CalendarUsers
                .FirstOrDefaultAsync(c => c.CalendarId == calendarId && c.UserId == userId);

            return cu != null && cu.Permissions == CalendarUser.PermissionLevel.Owner;
        }
    }
}
