using Calender.Interface;
using Calender.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calender.Repositories
{
    public class CalendarUserRepo : ICalendarUser
    {
        private readonly DatabaseContext _context;

        public CalendarUserRepo(DatabaseContext context)
        {
            _context = context;
        }

        // Hent alle CalendarUsers
        public async Task<List<CalendarUser>> GetAllCalendarUsersAsync()
        {
            return await _context.CalendarUsers
                .Include(cu => cu.User)
                .Include(cu => cu.Calendar)
                .ToListAsync();
        }

        // Hent en enkelt CalendarUser
        public async Task<CalendarUser?> GetCalendarUserAsync(int calendarId, int userId)
        {
            return await _context.CalendarUsers
                .Include(cu => cu.User)
                .Include(cu => cu.Calendar)
                .FirstOrDefaultAsync(cu => cu.CalendarId == calendarId && cu.UserId == userId);
        }

        // Opret en ny CalendarUser
        public async Task AddCalendarUserAsync(CalendarUser calendarUser)
        {
            _context.CalendarUsers.Add(calendarUser);
            await _context.SaveChangesAsync();
        }

        // Opdater en CalendarUser
        public async Task UpdateCalendarUserAsync(CalendarUser updatedCalendarUser)
        {
            var existingCalendarUser = await _context.CalendarUsers
                .FirstOrDefaultAsync(cu => cu.CalendarId == updatedCalendarUser.CalendarId && cu.UserId == updatedCalendarUser.UserId);

            if (existingCalendarUser == null)
                throw new KeyNotFoundException("CalendarUser not found.");

            existingCalendarUser.Permissions = updatedCalendarUser.Permissions;
            await _context.SaveChangesAsync();
        }

        // Slet en CalendarUser
        public async Task DeleteCalendarUserAsync(int calendarId, int userId)
        {
            var calendarUser = await _context.CalendarUsers
                .FirstOrDefaultAsync(cu => cu.CalendarId == calendarId && cu.UserId == userId);

            if (calendarUser != null)
            {
                _context.CalendarUsers.Remove(calendarUser);
                await _context.SaveChangesAsync();
            }
        }
    }
}
