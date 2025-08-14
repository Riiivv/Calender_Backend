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
    public class EventInvitationController : ControllerBase
    {
        private readonly EventInvitationRepo _eventInvitationRepo;
        private readonly DatabaseContext _context;

        public EventInvitationController(DatabaseContext context)
        {
            _context = context;
            _eventInvitationRepo = new EventInvitationRepo(context);
        }

        // Kun WebsiteAdmin
        [Authorize(Roles = "WebsiteAdmin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventInvitation>>> GetAllEventInvitations()
        {
            var invitations = await _eventInvitationRepo.GetAllEventInvitationsAsync();
            return Ok(invitations);
        }

        // Se invitation (recipient, sender eller admin)
        [Authorize]
        [HttpGet("{eventId}/{recipientId}")]
        public async Task<ActionResult<EventInvitation>> GetEventInvitation(int eventId, int recipientId)
        {
            var invitation = await _eventInvitationRepo.GetEventInvitationAsync(eventId, recipientId);
            if (invitation == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin" &&
                userId != invitation.RecipientId &&
                userId != invitation.SenderId)
            {
                return Forbid("Du har ikke adgang til denne invitation.");
            }

            return Ok(invitation);
        }

        // Opret invitation – kræver CanInvite på kalenderen
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<EventInvitation>> CreateEventInvitation(EventInvitation invitation)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (invitation.SenderId <= 0 || invitation.RecipientId <= 0 || invitation.EventId <= 0)
                return BadRequest("Invalid sender, recipient or event ID.");

            // Find kalenderId via event
            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == invitation.EventId);
            if (evt == null) return BadRequest("Event not found");

            if (role != "WebsiteAdmin")
            {
                var permission = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == evt.CalendarId && cu.UserId == userId);

                if (permission == null || !permission.IsOwner)
                    return Forbid("Du har ikke rettighed til at sende invitationer til dette event.");
            }

            // Check for eksisterende invitation
            var exists = await _eventInvitationRepo.GetEventInvitationAsync(invitation.EventId, invitation.RecipientId);
            if (exists != null)
                return Conflict("An invitation already exists for this user and event.");

            invitation.SenderId = userId;

            await _eventInvitationRepo.AddEventInvitationAsync(invitation);

            return CreatedAtAction(nameof(GetEventInvitation), new
            {
                eventId = invitation.EventId,
                recipientId = invitation.RecipientId
            }, invitation);
        }

        // Opdater – kun afsender eller admin
        [Authorize]
        [HttpPut("{eventId}/{recipientId}")]
        public async Task<IActionResult> UpdateEventInvitation(int eventId, int recipientId, EventInvitation invitation)
        {
            if (eventId != invitation.EventId || recipientId != invitation.RecipientId)
                return BadRequest("Path variables do not match body values.");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var existing = await _eventInvitationRepo.GetEventInvitationAsync(eventId, recipientId);
            if (existing == null) return NotFound();

            if (role != "WebsiteAdmin" && userId != existing.SenderId)
                return Forbid("Kun afsenderen må opdatere denne invitation.");

            await _eventInvitationRepo.UpdateEventInvitationAsync(invitation);
            return NoContent();
        }

        // Slet – sender, recipient eller admin
        [Authorize]
        [HttpDelete("{eventId}/{recipientId}")]
        public async Task<IActionResult> DeleteEventInvitation(int eventId, int recipientId)
        {
            var invitation = await _eventInvitationRepo.GetEventInvitationAsync(eventId, recipientId);
            if (invitation == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin" &&
                userId != invitation.SenderId &&
                userId != invitation.RecipientId)
            {
                return Forbid("Du har ikke adgang til at slette denne invitation.");
            }

            await _eventInvitationRepo.DeleteEventInvitationAsync(eventId, recipientId);
            return NoContent();
        }

        [Authorize]
        [HttpPost("{eventId}/{recipientId}/accept")]
        public async Task<IActionResult> AcceptEventInvitation(int eventId, int recipientId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (userId != recipientId)
                return Forbid("Du kan kun acceptere invitationer sendt til dig.");

            var invitation = await _eventInvitationRepo.GetEventInvitationAsync(eventId, recipientId);
            if (invitation == null)
                return NotFound("Invitationen findes ikke.");

            // Tjek om brugeren allerede deltager
            var alreadyIn = await _context.EventUsers
                .AnyAsync(eu => eu.EventId == eventId && eu.UserId == userId);
            if (alreadyIn)
                return BadRequest("Du deltager allerede i eventet.");

            // Tilføj til EventUsers
            _context.EventUsers.Add(new EventUser
            {
                EventId = eventId,
                UserId = userId
            });

            await _context.SaveChangesAsync();

            // Fjern invitation
            await _eventInvitationRepo.DeleteEventInvitationAsync(eventId, recipientId);

            return Ok("Du er nu tilføjet til eventet.");
        }

        [Authorize]
        [HttpPost("{eventId}/{recipientId}/decline")]
        public async Task<IActionResult> DeclineEventInvitation(int eventId, int recipientId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (userId != recipientId)
                return Forbid("Du kan kun afvise invitationer sendt til dig.");

            var invitation = await _eventInvitationRepo.GetEventInvitationAsync(eventId, recipientId);
            if (invitation == null)
                return NotFound("Invitationen findes ikke.");

            await _eventInvitationRepo.DeleteEventInvitationAsync(eventId, recipientId);

            return Ok("Invitationen er afvist og slettet.");
        }


    }
}
