using Calender.Interface;
using Calender.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calender.Repositories
{
    public class UserRepo : IUser
    {
        private readonly DatabaseContext _context;

        public UserRepo(DatabaseContext context)
        {
            _context = context;
        }

        // Hent alle brugere
        public async Task<List<User>> GetAllUsersAsync()
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

            return users;
        }

        // Hent en enkelt bruger
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Calendars)
                .Include(u => u.SentInvitations)
                .Include(u => u.RecievedInvitations)
                .Include(u => u.SentEventInvitations)
                .Include(u => u.RecievedEventInvitations)
                .Include(u => u.EventUsers)
                .Include(u => u.CalendarUsers)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        // Opret en ny bruger
        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // Opdater en bruger
        public async Task UpdateUserAsync(User updatedUser)
        {
            var user = await _context.Users.FindAsync(updatedUser.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            user.Username = updatedUser.Username;
            if (!string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
                user.PasswordHash = updatedUser.PasswordHash;

            await _context.SaveChangesAsync();
        }

        // Slet en bruger
        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Calendars)
                .Include(u => u.CalendarUsers)
                .Include(u => u.SentInvitations)
                .Include(u => u.RecievedInvitations)
                .Include(u => u.SentEventInvitations)
                .Include(u => u.RecievedEventInvitations)
                .Include(u => u.EventUsers)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Trin 1: Fjern CalendarUsers, der peger på brugerens kalendere
            foreach (var calendar in user.Calendars)
            {
                var relatedUsers = await _context.CalendarUsers
                    .Where(cu => cu.CalendarId == calendar.CalendarId)
                    .ToListAsync();

                _context.CalendarUsers.RemoveRange(relatedUsers);
            }

            // Trin 2: Fjern brugerens egne links
            _context.CalendarUsers.RemoveRange(user.CalendarUsers);
            _context.CalendarInvitations.RemoveRange(user.SentInvitations);
            _context.CalendarInvitations.RemoveRange(user.RecievedInvitations);
            _context.EventInvitations.RemoveRange(user.SentEventInvitations);
            _context.EventInvitations.RemoveRange(user.RecievedEventInvitations);
            _context.EventUsers.RemoveRange(user.EventUsers);

            // Trin 3: (valgfrit) Fjern brugerens egne kalendere
            _context.Calendars.RemoveRange(user.Calendars);

            // Trin 4: Fjern brugeren
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
        }
    }
}
