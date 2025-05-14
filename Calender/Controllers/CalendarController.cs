using Calender.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public CalendarController(DatabaseContext context)
        {
            _context = context;
        }

        //GET: api/Calendar
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Calendar>>> GetCalendar()
        {
            return Ok(await _context.Calendars
                .Include(c => c.Events)
                .Include(c => c.CalendarUsers)
                .Include(c => c.Invitations)
                .ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Calendar>> GetCalendar(int id)
        {
            var calendar = await _context.Calendars
                .Include(c => c.Events)
                .Include(c => c.CalendarUsers)
                .Include(c => c.Invitations)
                .FirstOrDefaultAsync(c => c.CalendarId == id);

            if (calendar == null)
                return NotFound($"Calendar with ID {id} not found.");

            return Ok(calendar);
        }

        //post
        [HttpPost]
        public async Task<ActionResult<Calendar>> CreateCalendar(Calendar calendar)
        {
            var user = await _context.Users.FindAsync(calendar.Userid);
            if (user == null)
                return BadRequest($"User with ID {calendar.Userid} not found.");

            _context.Calendars.Add(calendar);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCalendar), new { id = calendar.CalendarId }, calendar);
        }

        //update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCalendar(int id, Calendar updateCalendar)
        {
            var calendar = await _context.Calendars.FindAsync(id);
            if (calendar == null)
                return NotFound($"Calendar with ID {id} not found.");

            calendar.CalendarName = updateCalendar.CalendarName;
            calendar.Userid = updateCalendar.Userid;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        //delete
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCalendar(int id)
        {
            var calendar = await _context.Calendars.FindAsync(id);
            if (calendar == null)
                return NotFound($"Calendar with ID {id} not found.");

            _context.Calendars.Remove(calendar);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/events")]
        public async Task<ActionResult<IEnumerable<Event>>> GetCalendarEvents(int id)
        {
            var events = await _context.Events
                .Where(e => e.CalendarId == id)
                .ToListAsync();

            if (!events.Any())
                return NotFound($"No events found for Calendar ID {id}.");

            return Ok(events);
        }

        [HttpGet("{id}/invitations")]
        public async Task<ActionResult<IEnumerable<CalendarInvitation>>> GetCalendarInvitations(int id)
        {
            var invitations = await _context.CalendarInvitations
                .Where(ci => ci.CalendarId == id)
                .ToListAsync();

            if (!invitations.Any())
                return NotFound($"No invitations found for Calendar ID {id}.");

            return Ok(invitations);
        }
    }
}
