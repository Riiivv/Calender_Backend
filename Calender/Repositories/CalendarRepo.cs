using Calender.Interface;
using Calender.Models;
using Microsoft.EntityFrameworkCore;

namespace Calender.Repositories
{
    public class CalendarRepo : ICalendar
    {
        private readonly DatabaseContext _context;

        public CalendarRepo(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Calendar>> GetCalendarsAsync()
        {
            try
            {
                return await _context.Calendars
                    .Include(c => c.Events)
                    .Include(c => c.CalendarUsers)
                        .ThenInclude(cu => cu.User)
                    .Include(c => c.Invitations)
                    .OrderBy(c => c.CalendarName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve calendars", ex);
            }
        }

        public async Task<Calendar?> GetCalendarByIdAsync(int id)
        {
            try
            {
                return await _context.Calendars
                    .Include(c => c.Events)
                    .Include(c => c.CalendarUsers)
                        .ThenInclude(cu => cu.User)
                    .Include(c => c.Invitations)
                    .FirstOrDefaultAsync(c => c.CalendarId == id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve calendar with ID {id}", ex);
            }
        }

        public async Task<List<Calendar>> GetCalendarsByUserIdAsync(int userId)
        {
            try
            {
                var calendarIds = await _context.CalendarUsers
                    .Where(cu => cu.UserId == userId)
                    .Select(cu => cu.CalendarId)
                    .ToListAsync();

                return await _context.Calendars
                    .Where(c => calendarIds.Contains(c.CalendarId))
                    .Include(c => c.CalendarUsers)
                        .ThenInclude(cu => cu.User)
                    .OrderBy(c => c.CalendarName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve calendars for user {userId}", ex);
            }
        }

        public async Task<int> AddCalendarAsync(Calendar calendar)
        {
            if (calendar == null)
                throw new ArgumentNullException(nameof(calendar));

            if (string.IsNullOrWhiteSpace(calendar.CalendarName))
                throw new ArgumentException("Calendar name cannot be empty", nameof(calendar.CalendarName));

            try
            {
                calendar.CalendarName = calendar.CalendarName.Trim();

                _context.Calendars.Add(calendar);
                await _context.SaveChangesAsync();
                return calendar.CalendarId;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create calendar", ex);
            }
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.UserId == userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to check if user {userId} exists", ex);
            }
        }

        public async Task<bool> CalendarExistsAsync(int calendarId)
        {
            try
            {
                return await _context.Calendars.AnyAsync(c => c.CalendarId == calendarId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to check if calendar {calendarId} exists", ex);
            }
        }

        public async Task UpdateCalendarAsync(Calendar calendar)
        {
            if (calendar == null)
                throw new ArgumentNullException(nameof(calendar));

            if (string.IsNullOrWhiteSpace(calendar.CalendarName))
                throw new ArgumentException("Calendar name cannot be empty", nameof(calendar.CalendarName));

            try
            {
                var existingCalendar = await _context.Calendars.FindAsync(calendar.CalendarId);
                if (existingCalendar == null)
                    throw new KeyNotFoundException($"Calendar with ID {calendar.CalendarId} not found.");

                existingCalendar.CalendarName = calendar.CalendarName.Trim();
                // Note: We don't update Userid here to prevent ownership changes through this method

                await _context.SaveChangesAsync();
            }
            catch (KeyNotFoundException)
            {
                throw; // Re-throw KeyNotFoundException as-is
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update calendar {calendar.CalendarId}", ex);
            }
        }

        public async Task DeleteCalendarAsync(int id)
        {
            try
            {
                var calendar = await _context.Calendars
                    .Include(c => c.Events)
                    .Include(c => c.CalendarUsers)
                    .Include(c => c.Invitations)
                    .FirstOrDefaultAsync(c => c.CalendarId == id);

                if (calendar == null)
                    throw new KeyNotFoundException($"Calendar with ID {id} not found");

                // The related entities should be deleted automatically due to cascade delete
                // but we can be explicit about it if needed
                _context.Calendars.Remove(calendar);
                await _context.SaveChangesAsync();
            }
            catch (KeyNotFoundException)
            {
                throw; // Re-throw KeyNotFoundException as-is
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete calendar {id}", ex);
            }
        }

        public async Task<List<CalendarUser>> GetCalendarMembersAsync(int calendarId)
        {
            try
            {
                return await _context.CalendarUsers
                    .Where(cu => cu.CalendarId == calendarId)
                    .Include(cu => cu.User)
                    .OrderBy(cu => cu.User!.Username)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve members for calendar {calendarId}", ex);
            }
        }

        public async Task<CalendarUser.PermissionLevel?> GetUserPermissionAsync(int calendarId, int userId)
        {
            try
            {
                return await _context.CalendarUsers
                    .Where(cu => cu.CalendarId == calendarId && cu.UserId == userId)
                    .Select(cu => cu.Permissions)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve permissions for user {userId} in calendar {calendarId}", ex);
            }
        }
    }
}