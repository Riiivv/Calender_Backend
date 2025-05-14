using Calender.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public UserController(DatabaseContext context)
        {
            _context = context;
        }

        // Hent alle brugere
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.Calendars)
                .Include(u => u.SentInvitations)
                .Include(u => u.RecievedInvitations)
                .Include(u => u.SentEventInvitations)
                .Include(u => u.RecievedEventInvitations)
                .Include(u => u.EventUsers)
                .Include(u => u.CalendarUsers)
                .ToListAsync();

            return Ok(users);
        }

        // Hent en enkelt bruger
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _context.Users
                .Include(u => u.Calendars)
                .Include(u => u.SentInvitations)
                .Include(u => u.RecievedInvitations)
                .Include(u => u.SentEventInvitations)
                .Include(u => u.RecievedEventInvitations)
                .Include(u => u.EventUsers)
                .Include(u => u.CalendarUsers)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // Opret en ny bruger
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.PasswordHash))
                return BadRequest("Username and PasswordHash are required.");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, user);
        }

        // Opdater en bruger
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Username = updatedUser.Username;

            // Opdater kun password, hvis det er ændret
            if (!string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
                user.PasswordHash = updatedUser.PasswordHash;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Slet en bruger
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Calendars)
                .Include(u => u.SentInvitations)
                .Include(u => u.RecievedInvitations)
                .Include(u => u.SentEventInvitations)
                .Include(u => u.RecievedEventInvitations)
                .Include(u => u.EventUsers)
                .Include(u => u.CalendarUsers)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
