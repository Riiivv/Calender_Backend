using Calender.Models;
using Calender.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Linq.Expressions;

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
            if (string.IsNullOrWhiteSpace(evt.EventTitle))
                return BadRequest("EventTitle is required");

            if (evt.EventStart >= evt.EventEnd)
                return BadRequest("Event start must be before event end.");

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

            evt.EventId = id;

            try
            {
                await _eventRepo.UpdateEventAsync(evt);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Slet et event
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                await _eventRepo.DeleteEventAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
