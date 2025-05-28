using Calender.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Calender.Repositories;
using System.Runtime.CompilerServices;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserRepo _userRepo;

        public UserController(DatabaseContext context)
        {
            _context = context;
            _userRepo = new UserRepo(context);

        }

        // Hent alle brugere
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await _userRepo.GetAllUsersAsync();
        }

        // Hent en enkelt bruger
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);

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

            await _userRepo.AddUserAsync(user);

            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, user);
        }

        // Opdater en bruger
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            if (string.IsNullOrWhiteSpace(updatedUser.Username))
                return BadRequest("Username cannot be empy");

            if (string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
                return BadRequest("Password is required");

            updatedUser.UserId = id;

            try
            {
                await _userRepo.UpdateUserAsync(updatedUser);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // Slet en bruger
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userRepo.DeleteUserAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

        }
    }
}