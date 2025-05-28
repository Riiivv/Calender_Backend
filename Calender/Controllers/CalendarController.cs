using Calender.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Calender.Repositories;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly CalendarRepo _calendarRepo;

        public CalendarController(DatabaseContext context)
        {
            _context = context;
            _calendarRepo = new CalendarRepo(context);
        }


        //GET: api/Calendar
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Calendar>>> GetCalendar()
        {
            return await _calendarRepo.GetCalendarsAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Calendar>> GetCalendar(int id)
        {
            var calendar = await _calendarRepo.GetCalendarByIdAsync(id);

            if (calendar == null)
                return NotFound($"Calendar with ID {id} not found.");

            return Ok(calendar);
        }

        //post
        [HttpPost]
        public async Task<ActionResult<Calendar>> CreateCalendar(Calendar calendar)
        {

            if (calendar == null || string.IsNullOrWhiteSpace(calendar.CalendarName))
            return BadRequest("CalendarName is required.");

            if (!await _calendarRepo.UserExistsAsync(calendar.Userid))
                return BadRequest($"User with ID {calendar.Userid} Does not exist.");

            await _calendarRepo.AddCalendarAsync(calendar);

            return CreatedAtAction(nameof(GetCalendar), new { id = calendar.CalendarId }, calendar);
        }

        //update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCalendar(int id, Calendar updateCalendar)
        {
            if (string.IsNullOrWhiteSpace(updateCalendar.CalendarName))
                return BadRequest("Calenername is required.");

            updateCalendar.CalendarId = id;

            try
            {
                await _calendarRepo.UpdateCalendarAsync(updateCalendar);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //delete
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCalendar(int id)
        {
            try
            {
                await _calendarRepo.DeleteCalendarAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/events")]
        public async Task<ActionResult<IEnumerable<Event>>> GetCalendarEvents(int id)
        {
            var events = await _context.Events
                .Where(e => e.CalendarId == id)
                .ToListAsync();

            if (!events.Any())
                return NotFound($"No events found for Calendar ID {id}.");

            return Ok(events);
        }

        [HttpGet("{id}/invitations")]
        public async Task<ActionResult<IEnumerable<CalendarInvitation>>> GetCalendarInvitations(int id)
        {
            var invitations = await _context.CalendarInvitations
                .Where(ci => ci.CalendarId == id)
                .ToListAsync();

            if (!invitations.Any())
                return NotFound($"No invitations found for Calendar ID {id}.");

            return Ok(invitations);
        }
    }
}
