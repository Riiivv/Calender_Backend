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
    public class EventController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly EventRepo _eventRepo;

        public EventController(DatabaseContext context)
        {
            _context = context;
            _eventRepo = new EventRepo(context);
        }

        // Hent alle events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents()
        {
            return Ok(await _eventRepo.GetAllEventsAsync());
        }

        // Hent et enkelt event
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var evt = await _eventRepo.GetEventByIdAsync(id);
            if (evt == null) return NotFound();
            return Ok(evt);
        }

        // Opret et nyt event
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event evt)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (evt.EventStart >= evt.EventEnd)
                return BadRequest("Event start must be before event end.");

            // WebsiteAdmin må alt
            if (role != "WebsiteAdmin")
            {
                var permission = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == evt.CalendarId && cu.UserId == userId);

                if (permission == null || !permission.CanEdit)
                    return StatusCode(403, "Du har ikke rettighed til at oprette event i denne kalender.");
            }

            await _eventRepo.AddEventAsync(evt);
            return CreatedAtAction(nameof(GetEvent), new { id = evt.EventId }, evt);
        }

        // Opdater et event
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, Event evt)
        {
            if (string.IsNullOrWhiteSpace(evt.EventTitle))
                return BadRequest("EventTitle is required.");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var existingEvent = await _eventRepo.GetEventByIdAsync(id);
            if (existingEvent == null) return NotFound();

            // WebsiteAdmin = fuld adgang
            if (role != "WebsiteAdmin")
            {
                var permission = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == existingEvent.CalendarId && cu.UserId == userId);

                if (permission == null || !permission.CanEdit)
                    return StatusCode(403, "Du har ikke adgang til at redigere dette event.");
            }

            existingEvent.EventTitle = evt.EventTitle;
            existingEvent.EventStart = evt.EventStart;
            existingEvent.EventEnd = evt.EventEnd;

            await _eventRepo.UpdateEventAsync(existingEvent);
            return NoContent();
        }

        // Slet et event
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var evt = await _eventRepo.GetEventByIdAsync(id);
            if (evt == null) return NotFound();

            if (role != "WebsiteAdmin")
            {
                var permission = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == evt.CalendarId && cu.UserId == userId);

                if (permission == null || !permission.IsOwner)
                    return StatusCode(403, "Kun ejeren af kalenderen må slette events.");
            }

            await _eventRepo.DeleteEventAsync(id);
            return NoContent();
        }
    }
}
