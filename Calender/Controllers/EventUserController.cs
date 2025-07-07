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
    public class EventUserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly EventUserRepo _eventUserRepo;

        public EventUserController(DatabaseContext context)
        {
            _context = context;
            _eventUserRepo = new EventUserRepo(context);
        }

        // Kun WebsiteAdmin må hente alle
        [Authorize(Roles = "WebsiteAdmin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventUser>>> GetAllEventUsers()
        {
            var result = await _eventUserRepo.GetAllEventUsersAsync();
            return Ok(result);
        }

        // Hent én EventUser (kun dig selv, WebsiteAdmin, eller kalender-ejer)
        [Authorize]
        [HttpGet("{userId:int}/{eventId:int}")]
        public async Task<ActionResult<EventUser>> GetEventUser(int eventId, int userId)
        {
            var result = await _eventUserRepo.GetEventUserAsync(eventId, userId);
            if (result == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var calendarId = await _context.Events
                .Where(e => e.EventId == eventId)
                .Select(e => e.CalendarId)
                .FirstOrDefaultAsync();

            var permission = await _context.CalendarUsers
                .FirstOrDefaultAsync(cu => cu.CalendarId == calendarId && cu.UserId == currentUserId);

            if (role != "WebsiteAdmin" &&
                currentUserId != userId &&
                (permission == null || !permission.IsOwner))
                return Forbid("Du har ikke adgang til denne bruger.");

            return Ok(result);
        }

        // Opret EventUser – efter invitation eller admin
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<EventUser>> CreateEventUser(EventUser eventUser)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (eventUser == null)
                return BadRequest("Invalid event user data.");

            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventUser.EventId);
            if (evt == null) return BadRequest("Event not found.");

            if (role != "WebsiteAdmin")
            {
                var permission = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == evt.CalendarId && cu.UserId == userId);

                if (permission == null || !permission.IsOwner)
                    return Forbid("Du har ikke adgang til at tilføje brugere til dette event.");
            }

            try
            {
                await _eventUserRepo.AddEventUserAsync(eventUser);
                return CreatedAtAction(nameof(GetEventUser), new { eventId = eventUser.EventId, userId = eventUser.UserId }, eventUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Opdater – kun Owner eller WebsiteAdmin
        [Authorize]
        [HttpPut("{userId:int}/{eventId:int}")]
        public async Task<IActionResult> UpdateEventUser(int userId, int eventId, EventUser updateUser)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt == null) return NotFound("Event not found.");

            var permission = await _context.CalendarUsers
                .FirstOrDefaultAsync(cu => cu.CalendarId == evt.CalendarId && cu.UserId == currentUserId);

            if (role != "WebsiteAdmin" && (permission == null || !permission.IsOwner))
                return Forbid("Du må ikke ændre permissions på denne deltager.");

            updateUser.EventId = eventId;
            updateUser.UserId = userId;

            try
            {
                await _eventUserRepo.UpdateEventUserAsync(updateUser);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Slet EventUser – brugeren selv, Owner, eller WebsiteAdmin
        [Authorize]
        [HttpDelete("{userId:int}/{eventId:int}")]
        public async Task<IActionResult> DeleteEventUser(int userId, int eventId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt == null) return NotFound("Event not found.");

            var permission = await _context.CalendarUsers
                .FirstOrDefaultAsync(cu => cu.CalendarId == evt.CalendarId && cu.UserId == currentUserId);

            if (role != "WebsiteAdmin" &&
                currentUserId != userId &&
                (permission == null || !permission.IsOwner))
                return Forbid("Du har ikke adgang til at fjerne denne deltager.");

            try
            {
                await _eventUserRepo.DeleteEventUserAsync(eventId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Deltager ikke fundet.");
            }
        }
    }
}
