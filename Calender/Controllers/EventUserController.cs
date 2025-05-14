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

        // Hent alle EventUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventUser>>> GetAllEventUsers()
        {
            var eventUsers = await _context.EventUsers
                .Include(eu => eu.User)
                .Include(eu => eu.Event)
                .ToListAsync();

            return Ok(eventUsers);
        }

        // Hent en enkelt EventUser
        [HttpGet("{userId}/{eventId}")]
        public async Task<ActionResult<EventUser>> GetEventUser(int userId, int eventId)
        {
            var eventUser = await _context.EventUsers
                .Include(eu => eu.User)
                .Include(eu => eu.Event)
                .FirstOrDefaultAsync(eu => eu.UserId == userId && eu.EventId == eventId);

            if (eventUser == null)
                return NotFound();

            return Ok(eventUser);
        }

        // Opret en EventUser
        [HttpPost]
        public async Task<ActionResult<EventUser>> CreateEventUser(EventUser eventUser)
        {
            if (eventUser == null)
                return BadRequest();

            // Valider fremmednøgler
            var userExists = await _context.Users.AnyAsync(u => u.UserId == eventUser.UserId);
            var eventExists = await _context.Events.AnyAsync(e => e.EventId == eventUser.EventId);

            if (!userExists || !eventExists)
                return BadRequest("User or event Dosen't exist.");

            _context.EventUsers.Add(eventUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventUser), new { userId = eventUser.UserId, eventId = eventUser.EventId }, eventUser);
        }

        // Opdater en EventUser (kun permissions)
        [HttpPut("{userId}/{eventId}")]
        public async Task<IActionResult> UpdateEventUser(int userId, int eventId, EventUser updateUser)
        {
            var eventUser = await _context.EventUsers
                .Include(eu => eu.User)
                .Include(eu => eu.Event)
                .FirstOrDefaultAsync(eu => eu.UserId == userId && eu.EventId == eventId);

            if (eventUser == null)
                return NotFound();

            // Opdater kun permissions
            eventUser.Permissions = updateUser.Permissions;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Slet en EventUser
        [HttpDelete("{userId}/{eventId}")]
        public async Task<IActionResult> DeleteEventUser(int userId, int eventId)
        {
            var eventUser = await _context.EventUsers
                .Include(eu => eu.User)
                .Include(eu => eu.Event)
                .FirstOrDefaultAsync(eu => eu.UserId == userId && eu.EventId == eventId);

            if (eventUser == null)
                return NotFound();

            _context.EventUsers.Remove(eventUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
