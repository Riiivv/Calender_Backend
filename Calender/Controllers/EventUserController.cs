using Calender.Models;
using Calender.Repositories;
using Microsoft.AspNetCore.Authorization;
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
        private readonly EventUserRepo _eventUserRepo;

        public EventUserController(DatabaseContext context)
        {
            _context = context;
            _eventUserRepo = new EventUserRepo(context);
        }

        // Hent alle EventUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventUser>>> GetAllEventUsers()
        {
            var result = await _eventUserRepo.GetAllEventUsersAsync();
            return Ok(result);
        }

        // Hent en enkelt EventUser
        [HttpGet("{userId}/{eventId}")]
        public async Task<ActionResult<EventUser>> GetEventUser(int eventId, int userId)
        {
            var result = await _eventUserRepo.GetEventUserAsync(eventId, userId);
            if (result == null) return NotFound();
                return Ok(result);
        }


        // Opret en EventUser
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<EventUser>> CreateEventUser(EventUser eventUser)
        {
            if (eventUser == null)
                return BadRequest("Invalid event user data.");

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

        // Opdater en EventUser (kun permissions)
        [Authorize]
        [HttpPut("{userId}/{eventId}")]
        public async Task<IActionResult> UpdateEventUser(int userId, int eventId, EventUser updateUser)
        {
            if (updateUser == null)
                return BadRequest("Invalud input");

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

        // Slet en EventUser
        [Authorize]
        [HttpDelete("{userId}/{eventId}")]
        public async Task<IActionResult> DeleteEventUser(int userId, int eventId)
        {
            try
            {
                await _eventUserRepo.DeleteEventUserAsync(eventId, userId);
                    return NoContent();
            }
            catch (KeyNotFoundException) 
            {
                return NotFound();
            }
        }
    }
}
