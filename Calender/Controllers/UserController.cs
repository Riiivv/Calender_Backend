using Calender.DTO;
using Calender.Models;
using Calender.Repositories;
using Calender.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserRepo _userRepo;

        public UserController(DatabaseContext context)
        {
            _userRepo = new UserRepo(context);
        }

        // Hent alle brugere (kun id + username)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return Ok(users.Select(u => u.ToDTO()));
        }

        // Hent en bruger (kun id + username)
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            return Ok(user.ToDTO());
        }

        // Opret ny bruger
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.PasswordHash))
                return BadRequest("Username and PasswordHash are required.");

            await _userRepo.AddUserAsync(user);

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user.ToDTO());
        }

        // Opdater eksisterende bruger
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            if (string.IsNullOrWhiteSpace(updatedUser.Username))
                return BadRequest("Username is required.");

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
