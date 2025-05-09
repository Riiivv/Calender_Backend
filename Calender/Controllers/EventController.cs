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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents()
        {
            return Ok(await _context.Events.ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event eevent)
        {
            if (eevent == null)
                return BadRequest();

            _context.Events.Add(eevent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllEvents), new { id = eevent.EventId }, eevent);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<Event>> UpdateEvent(int id, Event eventUpdate)
        {
            var uevent = await _context.Events.FindAsync(id);
            if (uevent == null)
                return NotFound();

            uevent.CalendarId = eventUpdate.CalendarId;
            uevent.EventDescription = eventUpdate.EventDescription;
            uevent.EventTitle = eventUpdate.EventTitle;
            uevent.EventStart = eventUpdate.EventStart;
            uevent.EventEnd = eventUpdate.EventEnd;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            var devent = await _context.Events.FindAsync(id);
            if (devent == null)
                return NotFound();

            _context.Events.Remove(devent);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
