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

        // Hent alle Event Invitations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventInvitation>>> GetAllEventInvitations()
        {
            var invitations = await _context.EventInvitations
                .Include(ei => ei.Sender)
                .Include(ei => ei.Recipient)
                .Include(ei => ei.Event)
                .ToListAsync();

            return Ok(invitations);
        }

        // Hent en enkelt Event Invitation
        [HttpGet("{eventId}/{recipientId}")]
        public async Task<ActionResult<EventInvitation>> GetEventInvitation(int eventId, int recipientId)
        {
            var invitation = await _context.EventInvitations
                .Include(ei => ei.Sender)
                .Include(ei => ei.Recipient)
                .Include(ei => ei.Event)
                .FirstOrDefaultAsync(ei => ei.EventId == eventId && ei.RecipientId == recipientId);

            if (invitation == null)
                return NotFound();

            return Ok(invitation);
        }

        // Opret en Event Invitation
        [HttpPost]
        public async Task<ActionResult<EventInvitation>> CreateEventInvitation(EventInvitation invitation)
        {
            if (invitation == null)
                return BadRequest();

            // Valider fremmednøgler
            var senderExists = await _context.Users.AnyAsync(u => u.UserId == invitation.SenderId);
            var recipientExists = await _context.Users.AnyAsync(u => u.UserId == invitation.RecipientId);
            var eventExists = await _context.Events.AnyAsync(e => e.EventId == invitation.EventId);

            if (!senderExists || !recipientExists || !eventExists)
                return BadRequest("Sender, Recipient or Event dosen't exist.");

            _context.EventInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventInvitation), new { eventId = invitation.EventId, recipientId = invitation.RecipientId }, invitation);
        }

        // Slet en Event Invitation
        [HttpDelete("{eventId}/{recipientId}")]
        public async Task<IActionResult> DeleteEventInvitation(int eventId, int recipientId)
        {
            var invitation = await _context.EventInvitations
                .Include(ei => ei.Sender)
                .Include(ei => ei.Recipient)
                .Include(ei => ei.Event)
                .FirstOrDefaultAsync(ei => ei.EventId == eventId && ei.RecipientId == recipientId);

            if (invitation == null)
                return NotFound();

            _context.EventInvitations.Remove(invitation);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
