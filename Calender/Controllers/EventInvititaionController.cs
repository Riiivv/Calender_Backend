using Calender.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventInvitationController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public EventInvitationController(DatabaseContext context)
        {
            _context = context;
        }

        // GET all Event Invitations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventInvitation>>> GetAllEventInvitations()
        {
            return Ok(await _context.EventInvitations.ToListAsync());
        }

        // GET Event Invitation by ID
        [HttpGet("{eventId}/{recipientId}")]
        public async Task<ActionResult<EventInvitation>> GetEventInvitation(int eventId, int recipientId)
        {
            var invitation = await _context.EventInvitations
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.RecipientId == recipientId);

            if (invitation == null)
                return NotFound();

            return Ok(invitation);
        }

        // POST Event Invitation
        [HttpPost]
        public async Task<ActionResult<EventInvitation>> CreateEventInvitation(EventInvitation invitation)
        {
            if (invitation == null)
                return BadRequest();

            _context.EventInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventInvitation), new { eventId = invitation.EventId, recipientId = invitation.RecipientId }, invitation);
        }

        // DELETE Event Invitation
        [HttpDelete("{eventId}/{recipientId}")]
        public async Task<ActionResult> DeleteEventInvitation(int eventId, int recipientId)
        {
            var invitation = await _context.EventInvitations
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.RecipientId == recipientId);

            if (invitation == null)
                return NotFound();

            _context.EventInvitations.Remove(invitation);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
