using Calender.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public CalendarController(DatabaseContext context)
        {
            _context = context;
        }

        //GET: api/Calendar
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Calendar>>> GetCalendar()
        {
            return await _context.Calendars.ToListAsync();
        }
        //post
        [HttpPost]
        public async Task<ActionResult<Calendar>> CreateCalener(Calendar calendar)
        {
            if (calendar is null)
                return BadRequest();

            _context.Calendars.Add(calendar);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCalendar), new { id = calendar.CalendarId }, calendar);
        }
        //update
        [HttpPut("{id}")]
        public async Task<ActionResult<Calendar>> Updatecalender(int id, Calendar updateCalendar)
        {
            var calendar = await _context.Calendars.FindAsync(id);
            if (calendar is null)
                return NotFound();

            calendar.Userid = updateCalendar.Userid;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        //delete
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCalender(int id)
        {
            var calendar = await _context.Calendars.FindAsync(id);
            if (calendar is null)
                return NotFound();

            _context.Calendars.Remove(calendar);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
