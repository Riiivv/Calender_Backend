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
    public class CalendarInvitationController : ControllerBase
    {
        private readonly CalendarInvitationRepo _invitationRepo;
        private readonly DatabaseContext _context;

        public CalendarInvitationController(DatabaseContext context)
        {
            _invitationRepo = new CalendarInvitationRepo(context);
            _context = context;
        }

        // Kun WebsiteAdmin
        [Authorize(Roles = "WebsiteAdmin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalendarInvitation>>> GetAllInvitations()
        {
            var invitations = await _invitationRepo.GetAllCalendarInvitationsAsync();
            return Ok(invitations);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<CalendarInvitation>> GetInvitationById(int id)
        {
            var invitation = await _invitationRepo.GetCalendarInvitationAsync(id);
            if (invitation == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin" &&
                invitation.RecipientId != userId &&
                invitation.SenderId != userId)
            {
                var permission = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == invitation.CalendarId && cu.UserId == userId);

                if (permission == null || !permission.IsOwner)
                    return Forbid("Du har ikke adgang til denne invitation.");
            }

            return Ok(invitation);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CalendarInvitation>> CreateInvitation(CalendarInvitation invitation)
        {
            if (invitation == null)
                return BadRequest("Invitation cannot be null.");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin")
            {
                var permission = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == invitation.CalendarId && cu.UserId == userId);

                if (permission == null || !(permission.IsOwner || permission.Permissions == CalendarUser.PermissionLevel.Moderator))
                    return Forbid("Du har ikke rettighed til at invitere brugere.");
            }

            invitation.SenderId = userId;

            await _invitationRepo.AddCalendarInvitationAsync(invitation);
            return CreatedAtAction(nameof(GetInvitationById), new { id = invitation.InvitationId }, invitation);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvitation(int id, CalendarInvitation updatedInvitation)
        {
            if (id != updatedInvitation.InvitationId)
                return BadRequest("ID mismatch.");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var existing = await _invitationRepo.GetCalendarInvitationAsync(id);
            if (existing == null)
                return NotFound();

            if (role != "WebsiteAdmin" && existing.SenderId != userId)
                return Forbid("Kun afsenderen kan opdatere invitationen.");

            await _invitationRepo.UpdateCalendarInvitationAsync(updatedInvitation);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvitation(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var invitation = await _invitationRepo.GetCalendarInvitationAsync(id);
            if (invitation == null)
                return NotFound($"Invitation with ID {id} not found.");

            if (role != "WebsiteAdmin" && invitation.SenderId != userId)
                return Forbid("Kun afsenderen kan slette invitationen.");

            await _invitationRepo.DeleteCalendarInvitationAsync(id);
            return NoContent();
        }

        [Authorize]
        [HttpGet("{id}/recipient")]
        public async Task<ActionResult<User>> GetRecipient(int id)
        {
            var invitation = await _invitationRepo.GetCalendarInvitationAsync(id);
            if (invitation == null) return NotFound("Invitation not found");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin" &&
                 invitation.RecipientId != userId &&
                 invitation.SenderId != userId)
            {
                var permission = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == invitation.CalendarId && cu.UserId == userId);

                if (permission == null || !permission.IsOwner)
                    return Forbid("Ingen adgang til denne invitation.");
            }

            var recipient = await _invitationRepo.GetRecipientByCalendarInvitationAsync(id);
            if (recipient == null)
                return NotFound("Recipient not found.");

            return Ok(recipient);
        }

        [Authorize]
        [HttpGet("{id}/sender")]
        public async Task<ActionResult<User>> GetSender(int id)
        {
            var invitation = await _invitationRepo.GetCalendarInvitationAsync(id);
            if (invitation == null) return NotFound("Invitation not found.");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "WebsiteAdmin" &&
                invitation.RecipientId != userId &&
                invitation.SenderId != userId)
            {
                var permission = await _context.CalendarUsers
                    .FirstOrDefaultAsync(cu => cu.CalendarId == invitation.CalendarId && cu.UserId == userId);

                if (permission == null || !permission.IsOwner)
                    return Forbid("Ingen adgang til denne invitation.");
            }

            var sender = await _invitationRepo.GetSenderByCalendarInvitationAsync(id);
            if (sender == null)
                return NotFound("Sender not found.");

            return Ok(sender);
        }

        [Authorize]
        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptInvitation(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var invitation = await _invitationRepo.GetCalendarInvitationAsync(id);
            if (invitation == null)
                return NotFound("Invitation not found.");

            if (invitation.RecipientId != userId)
                return StatusCode(403, "Du kan kun acceptere invitationer, der er sendt til dig.");

            var alreadyMember = await _context.CalendarUsers
                .AnyAsync(cu => cu.CalendarId == invitation.CalendarId && cu.UserId == userId);

            if (alreadyMember)
                return BadRequest("Du er allerede medlem af denne kalender.");

            var calendarUser = new CalendarUser
            {
                CalendarId = invitation.CalendarId,
                UserId = userId,
                Permissions = CalendarUser.PermissionLevel.User
            };

            _context.CalendarUsers.Add(calendarUser);
            await _context.SaveChangesAsync();

            await _invitationRepo.DeleteCalendarInvitationAsync(id);

            return Ok("Invitation accepteret. Du er nu tilføjet til kalenderen.");
        }

        [Authorize]
        [HttpPost("{id}/decline")]
        public async Task<IActionResult> DeclineInvitation(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var invitation = await _invitationRepo.GetCalendarInvitationAsync(id);
            if (invitation == null)
                return NotFound("Invitation ikke fundet.");

            if (invitation.RecipientId != userId)
                return StatusCode(403, "Du kan kun afvise invitationer, der er sendt til dig.");

            await _invitationRepo.DeleteCalendarInvitationAsync(id);

            return Ok("Invitationen er afvist og slettet.");
        }

    }
}
