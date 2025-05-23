using Calender.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Calender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly DatabaseContext _context;
        IConfiguration configuration;
        public AuthController(DatabaseContext context, IConfiguration configuration)
        {
            this._context = context;
            this.configuration = configuration;
        }

        [HttpPost("register")]

        public async Task<ActionResult<User>> Register(User request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.PasswordHash))
                return BadRequest("Username and password are required");

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username already exists.");


            var user = new User
            {
                Username = request.Username
            };

            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, request.PasswordHash);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { user.UserId, user.Username });
        }

        [HttpPost("login")]

        public async Task<ActionResult<User>> login(User request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                return BadRequest("User or password is incorrect");

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.PasswordHash);

            if (result == PasswordVerificationResult.Failed)
                return BadRequest("User or password is incorrect");

            var token = CreateToken(user);
            return Ok(token);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AuthenticatedOnlyEndpoint()
        {

            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (username == null)
                return NotFound();

            return Ok(new
            {
                message = "You are authenticated",
                username,
                userId
            });
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Token"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("JwtSettings:ExpireMinutes")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
