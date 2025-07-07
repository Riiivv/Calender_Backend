using Calender.DTO;
using Calender.Models;
using Calender.Repositories;
using Calender.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Calender.Repositories;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Calender.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;

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

        [Authorize]
        // Hent en bruger (kun id + username)
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var currentUser = HttpContext.User;

            var role = currentUser.FindFirst(ClaimTypes.Role)?.Value;

            bool includeId = role == "Admin";
            return Ok(user.ToDTO(includeId));
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

        
        // Opdater en bruger
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            if (string.IsNullOrWhiteSpace(updatedUser.Username))
                return BadRequest("Username is required.");


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

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUser = HttpContext.User;

            var currentUserId = int.Parse(currentUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var role = currentUser.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            // Hvis ikke admin og forsøger at slette en anden bruger
            if (role != "WebsiteAdmin" && currentUserId != id)
            {
                return Forbid();
            }

            try
            {
                await _userRepo.DeleteUserAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Bruger ikke fundet.");
            }
        }

    }
}