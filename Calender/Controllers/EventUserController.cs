using Calender.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventUserController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public EventUserController(DatabaseContext context)
        {
            _context = context;
        }

        // GET all EventUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventUser>>> GetAllEventUsers()
        {
            return Ok(await _context.EventUsers.ToListAsync());
        }

        // GET single EventUser by UserId and EventId
        [HttpGet("{userId}/{eventId}")]
        public async Task<ActionResult<EventUser>> GetEventUser(int userId, int eventId)
        {
            var eventUser = await _context.EventUsers
                .FirstOrDefaultAsync(eu => eu.UserId == userId && eu.EventId == eventId);

            if (eventUser == null)
                return NotFound();

            return Ok(eventUser);
        }

        // POST EventUser
        [HttpPost]
        public async Task<ActionResult<EventUser>> CreateEventUser(EventUser eventUser)
        {
            if (eventUser == null)
                return BadRequest();

            _context.EventUsers.Add(eventUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventUser), new { userId = eventUser.UserId, eventId = eventUser.EventId }, eventUser);
        }

        // PUT EventUser (update permissions)
        [HttpPut("{userId}/{eventId}")]
        public async Task<ActionResult> UpdateEventUser(int userId, int eventId, EventUser updateUser)
        {
            var eventUser = await _context.EventUsers
                .FirstOrDefaultAsync(eu => eu.UserId == userId && eu.EventId == eventId);

            if (eventUser == null)
                return NotFound();

            // Update only permissions
            eventUser.Permissions = updateUser.Permissions;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE EventUser
        [HttpDelete("{userId}/{eventId}")]
        public async Task<ActionResult> DeleteEventUser(int userId, int eventId)
        {
            var eventUser = await _context.EventUsers
                .FirstOrDefaultAsync(eu => eu.UserId == userId && eu.EventId == eventId);

            if (eventUser == null)
                return NotFound();

            _context.EventUsers.Remove(eventUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
