using Calender.Models;
using Calender.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventInvitationController : ControllerBase
    {
        private readonly EventInvitationRepo _eventInvitationRepo;

        public EventInvitationController(DatabaseContext context)
        {
            _eventInvitationRepo = new EventInvitationRepo(context);
        }

        // Hent alle Event Invitations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventInvitation>>> GetAllEventInvitations()
        {
            var invitations = await _eventInvitationRepo.GetAllEventInvitationsAsync();
            return Ok(invitations);
        }

        // Hent én invitation
        [HttpGet("{eventId}/{recipientId}")]
        public async Task<ActionResult<EventInvitation>> GetEventInvitation(int eventId, int recipientId)
        {
            var invitation = await _eventInvitationRepo.GetEventInvitationAsync(eventId, recipientId);
            if (invitation == null)
                return NotFound();

            return Ok(invitation);
        }

        // Opret ny invitation
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<EventInvitation>> CreateEventInvitation(EventInvitation invitation)
        {
            if (invitation.SenderId <= 0 || invitation.RecipientId <= 0 || invitation.EventId <= 0)
                return BadRequest("Invalid sender, recipient or event ID.");

            var exists = await _eventInvitationRepo.GetEventInvitationAsync(invitation.EventId, invitation.RecipientId);
            if (exists != null)
                return Conflict("An invitation already exists for this user and event.");


            await _eventInvitationRepo.AddEventInvitationAsync(invitation);

            return CreatedAtAction(nameof(GetEventInvitation), new
            {
                eventId = invitation.EventId,
                recipientId = invitation.RecipientId
            }, invitation);
        }

        // Opdater invitation
        [Authorize]
        [HttpPut("{eventId}/{recipientId}")]
        public async Task<IActionResult> UpdateEventInvitation(int eventId, int recipientId, EventInvitation invitation)
        {
            if (eventId != invitation.EventId || recipientId != invitation.RecipientId)
                return BadRequest("Path variables do not match body values.");

            try
            {
                await _eventInvitationRepo.UpdateEventInvitationAsync(invitation);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Slet invitation
        [Authorize]
        [HttpDelete("{eventId}/{recipientId}")]
        public async Task<IActionResult> DeleteEventInvitation(int eventId, int recipientId)
        {
            var existing = await _eventInvitationRepo.GetEventInvitationAsync(eventId, recipientId);
            if (existing == null)
                return NotFound();

            await _eventInvitationRepo.DeleteEventInvitationAsync(eventId, recipientId);
            return NoContent();
        }
    }
}
