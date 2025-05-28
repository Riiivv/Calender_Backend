using Calender.Models;
using Calender.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarUserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly CalendarUserRepo _calendarUserRepo;

        public CalendarUserController(DatabaseContext context)
        {
            _context = context;
            _calendarUserRepo = new CalendarUserRepo(context);
        }

        // Hent alle CalendarUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalendarUser>>> GetAllCUsers()
        {
            var calendarUsers = await _calendarUserRepo.GetAllCalendarUsersAsync();
            return Ok(calendarUsers);
        }

        //hent en specefic CalendarUser
        [HttpGet("{CalendarId}/{UserId}")]
        public async Task<ActionResult<CalendarUser>> GetCalendarUser(int calendarid, int userId)
        {
            var calendarUser = await _calendarUserRepo.GetCalendarUserAsync(calendarid, userId);
            if (calendarUser == null)
                return NotFound();

            return Ok(calendarUser);
        }

        // Opret en ny CalendarUser
        [HttpPost]
        public async Task<ActionResult<CalendarUser>> CreateCUser(CalendarUser cuser)
        {
            if (cuser == null)
                return BadRequest();

            // Valider fremmednøgler
            var userExists = await _context.Users.AnyAsync(u => u.UserId == cuser.UserId);
            var calendarExists = await _context.Calendars.AnyAsync(c => c.CalendarId == cuser.CalendarId);

            if (!userExists || !calendarExists)
                return BadRequest("User or Calendar dosen't exist.");

            _context.CalendarUsers.Add(cuser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllCUsers), new { id = cuser.CalendarId }, cuser);
        }

        // Opdater en CalendarUser
        [HttpPut("{calendarId}/{userId}")]
        public async Task<IActionResult> UpdateCUser(int calendarId, int userId, CalendarUser updatecuser)
        {
            updatecuser.CalendarId = calendarId;
            updatecuser.UserId = userId;

            try
            {
                await _calendarUserRepo.UpdateCalendarUserAsync(updatecuser);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //nået her til

        // Slet en CalendarUser
        [HttpDelete("{calendarId}/{userId}")]
        public async Task<IActionResult> DeleteCuser(int calendarId, int userId)
        {
            try
            {
                await _calendarUserRepo.DeleteCalendarUserAsync(calendarId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
