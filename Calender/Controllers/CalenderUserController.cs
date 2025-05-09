using Calender.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalenderUserController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public CalenderUserController(DatabaseContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalendarUser>>> GetAllCUsers()
        {
            return Ok(await _context.CalendarUsers.ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult<CalendarUser>> CreateCUser(CalendarUser cuser)
        {
            if (cuser == null)
                return BadRequest();

            _context.CalendarUsers.Add(cuser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllCUsers), new { id = cuser.CalendarId }, cuser);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<CalendarUser>> UpdateCUser(int id, CalendarUser updatecuser)
        {
            var user = await _context.CalendarUsers.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Permissions = updatecuser.Permissions;


            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCuser(int id)
        {
            var cuser = await _context.CalendarUsers.FindAsync(id);
            if (cuser == null)
                return NotFound();

            _context.CalendarUsers.Remove(cuser);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
