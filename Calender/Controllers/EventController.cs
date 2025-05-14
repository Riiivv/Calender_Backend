using Calender.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public EventController(DatabaseContext context)
        {
            _context = context;
        }

        // Hent alle events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Calendar)
                .Include(e => e.EventUsers)
                .Include(e => e.Invitations)
                .ToListAsync();

            return Ok(events);
        }

        // Hent et enkelt event
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var eevent = await _context.Events
                .Include(e => e.Calendar)
                .Include(e => e.EventUsers)
                .Include(e => e.Invitations)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eevent == null)
                return NotFound();

            return Ok(eevent);
        }

        // Opret et nyt event
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event eevent)
        {
            if (eevent == null)
                return BadRequest();

            // Valider fremmednøgle (CalendarId)
            var calendarExists = await _context.Calendars.AnyAsync(c => c.CalendarId == eevent.CalendarId);
            if (!calendarExists)
                return BadRequest("Calendar dosen't exist.");

            _context.Events.Add(eevent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = eevent.EventId }, eevent);
        }

        // Opdater et event
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, Event eventUpdate)
        {
            var uevent = await _context.Events.FindAsync(id);
            if (uevent == null)
                return NotFound();

            // Opdater event data
            uevent.CalendarId = eventUpdate.CalendarId;
            uevent.EventDescription = eventUpdate.EventDescription;
            uevent.EventTitle = eventUpdate.EventTitle;
            uevent.EventStart = eventUpdate.EventStart;
            uevent.EventEnd = eventUpdate.EventEnd;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Slet et event
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var devent = await _context.Events
                .Include(e => e.EventUsers)
                .Include(e => e.Invitations)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (devent == null)
                return NotFound();

            _context.Events.Remove(devent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
