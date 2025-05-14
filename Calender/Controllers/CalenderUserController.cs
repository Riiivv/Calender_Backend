using Calender.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarUserController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public CalendarUserController(DatabaseContext context)
        {
            _context = context;
        }

        // Hent alle CalendarUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalendarUser>>> GetAllCUsers()
        {
            var calendarUsers = await _context.CalendarUsers
                .Include(cu => cu.User)
                .Include(cu => cu.Calendar)
                .ToListAsync();

            return Ok(calendarUsers);
        }

        // Opret en ny CalendarUser
        [HttpPost]
        public async Task<ActionResult<CalendarUser>> CreateCUser(CalendarUser cuser)
        {
            if (cuser == null)
                return BadRequest();

            // Valider fremmednøgler
            var userExists = await _context.Users.AnyAsync(u => u.UserId == cuser.UserId);
            var calendarExists = await _context.Calendars.AnyAsync(c => c.CalendarId == cuser.CalendarId);

            if (!userExists || !calendarExists)
                return BadRequest("User or Calendar dosen't exist.");

            _context.CalendarUsers.Add(cuser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllCUsers), new { id = cuser.CalendarId }, cuser);
        }

        // Opdater en CalendarUser
        [HttpPut("{calendarId}/{userId}")]
        public async Task<IActionResult> UpdateCUser(int calendarId, int userId, CalendarUser updatecuser)
        {
            var cuser = await _context.CalendarUsers
                .Include(cu => cu.User)
                .Include(cu => cu.Calendar)
                .FirstOrDefaultAsync(cu => cu.CalendarId == calendarId && cu.UserId == userId);

            if (cuser == null)
                return NotFound();

            cuser.Permissions = updatecuser.Permissions;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Slet en CalendarUser
        [HttpDelete("{calendarId}/{userId}")]
        public async Task<IActionResult> DeleteCuser(int calendarId, int userId)
        {
            var cuser = await _context.CalendarUsers
                .FirstOrDefaultAsync(cu => cu.CalendarId == calendarId && cu.UserId == userId);

            if (cuser == null)
                return NotFound();

            _context.CalendarUsers.Remove(cuser);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
