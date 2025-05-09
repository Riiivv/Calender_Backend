using Calender.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarInvitiationController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public CalendarInvitiationController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalendarInvitation>>> GetAllCalendarInvitations()
        {
            return Ok(await _context.CalendarInvitations.ToListAsync());
        }


        [HttpPost]
        public async Task<ActionResult<CalendarInvitation>> CreateInvitation(CalendarInvitation invitation)
        {
            if (invitation == null)
                return BadRequest();

            _context.CalendarInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllCalendarInvitations), new { id = invitation.InvitationId }, invitation);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CalendarInvitation>> UpdateCalendar(int id, CalendarInvitation updatecalendar)
        {
            var invitation = await _context.CalendarInvitations.FindAsync(id);
            if (invitation is null)
                return NotFound();

            invitation.SenderId = updatecalendar.SenderId;
            invitation.RecipientId = updatecalendar.RecipientId;
            invitation.CalendarId = updatecalendar.CalendarId;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteInvitation(int id)
        {
            var invitation = await _context.CalendarInvitations.FindAsync(id);
            if (invitation == null)
                return NotFound();

            _context.CalendarInvitations.Remove(invitation);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
