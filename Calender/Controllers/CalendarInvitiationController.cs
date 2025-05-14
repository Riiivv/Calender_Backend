using Calender.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarInvitationController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public CalendarInvitationController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalendarInvitation>>> GetAllCalendarInvitations()
        {
            var invitations = await _context.CalendarInvitations
                .Include(ci => ci.Sender)
                .Include(ci => ci.Recipient)
                .Include(ci => ci.Calendar)
                .ToListAsync();
            return Ok(invitations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CalendarInvitation>> GetCalendarInvitation(int id)
        {
            var invitation = await _context.CalendarInvitations
                .Include(ci => ci.Sender)
                .Include(ci => ci.Recipient)
                .Include(ci => ci.Calendar)
                .FirstOrDefaultAsync(ci => ci.InvitationId == id);

            if (invitation == null)
                return NotFound();

            return Ok(invitation);
        }

        [HttpPost]
        public async Task<ActionResult<CalendarInvitation>> CreateInvitation(CalendarInvitation invitation)
        {
            if (invitation == null)
                return BadRequest();

            var sender = await _context.Users.FindAsync(invitation.SenderId);
            var recipient = await _context.Users.FindAsync(invitation.RecipientId);
            var calendar = await _context.Calendars.FindAsync(invitation.CalendarId);

            if (sender == null || recipient == null || calendar == null)
                return BadRequest("Sender, Recipient or Calendar doesn't exist.");

            _context.CalendarInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Rette denne linje
            return CreatedAtAction(nameof(GetCalendarInvitation), new { id = invitation.InvitationId }, invitation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCalendarInvitation(int id, CalendarInvitation updatecalendar)
        {
            var invitation = await _context.CalendarInvitations
                .Include(ci => ci.Sender)
                .Include(ci => ci.Recipient)
                .Include(ci => ci.Calendar)
                .FirstOrDefaultAsync(ci => ci.InvitationId == id);  // Rette denne linje

            if (invitation == null)
                return NotFound();

            // Valider fremmednøgler (Sender, Recipient, Calendar)
            var sender = await _context.Users.FindAsync(updatecalendar.SenderId);
            var recipient = await _context.Users.FindAsync(updatecalendar.RecipientId);
            var calendar = await _context.Calendars.FindAsync(updatecalendar.CalendarId);

            if (sender == null || recipient == null || calendar == null)
                return BadRequest("Sender, Recipient or Calendar doesn't exist.");

            invitation.SenderId = updatecalendar.SenderId;
            invitation.RecipientId = updatecalendar.RecipientId;
            invitation.CalendarId = updatecalendar.CalendarId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvitation(int id)
        {
            var invitation = await _context.CalendarInvitations
                .Include(ci => ci.Sender)
                .Include(ci => ci.Recipient)
                .Include(ci => ci.Calendar)
                .FirstOrDefaultAsync(ci => ci.InvitationId == id);

            if (invitation == null)
                return NotFound();

            _context.CalendarInvitations.Remove(invitation);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
