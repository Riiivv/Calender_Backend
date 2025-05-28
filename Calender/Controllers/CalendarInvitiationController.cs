using Calender.Models;
using Calender.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarInvitationController : ControllerBase
    {
        private readonly CalendarInvitationRepo _invitationRepo;

        public CalendarInvitationController(DatabaseContext context)
        {
            _invitationRepo = new CalendarInvitationRepo(context);
        }

        // Hent alle invitationer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalendarInvitation>>> GetAllInvitations()
        {
            var invitations = await _invitationRepo.GetAllCalendarInvitationsAsync();
            return Ok(invitations);
        }

        // Hent én invitation
        [HttpGet("{id}")]
        public async Task<ActionResult<CalendarInvitation>> GetInvitationById(int id)
        {
            var invitation = await _invitationRepo.GetCalendarInvitationAsync(id);
            if (invitation == null)
                return NotFound();

            return Ok(invitation);
        }

        // Opret en invitation
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CalendarInvitation>> CreateInvitation(CalendarInvitation invitation)
        {
            if (invitation == null)
                return BadRequest("Invitation cannot be null.");

            await _invitationRepo.AddCalendarInvitationAsync(invitation);
            return CreatedAtAction(nameof(GetInvitationById), new { id = invitation.InvitationId }, invitation);
        }

        // Opdater en invitation
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvitation(int id, CalendarInvitation updatedInvitation)
        {
            if (id != updatedInvitation.InvitationId)
                return BadRequest("ID mismatch.");

            try
            {
                await _invitationRepo.UpdateCalendarInvitationAsync(updatedInvitation);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Slet en invitation
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvitation(int id)
        {
            try
            {
                await _invitationRepo.DeleteCalendarInvitationAsync(id);
                return NoContent();
            }
            catch
            {
                return NotFound($"Invitation with ID {id} not found.");
            }
        }

        // Hent modtageren (recipient) af en invitation
        [HttpGet("{id}/recipient")]
        public async Task<ActionResult<User>> GetRecipient(int id)
        {
            var recipient = await _invitationRepo.GetRecipientByCalendarInvitationAsync(id);
            if (recipient == null)
                return NotFound("Recipient not found.");
            return Ok(recipient);
        }

        // Hent afsenderen (sender) af en invitation
        [HttpGet("{id}/sender")]
        public async Task<ActionResult<User>> GetSender(int id)
        {
            var sender = await _invitationRepo.GetSenderByCalendarInvitationAsync(id);
            if (sender == null)
                return NotFound("Sender not found.");
            return Ok(sender);
        }
    }
}
