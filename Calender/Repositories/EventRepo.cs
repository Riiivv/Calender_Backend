using Calender.Interface;
using Calender.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calender.Repositories
{
    public class EventRepo : IEvent
    {
        private readonly DatabaseContext _context;

        public EventRepo(DatabaseContext context)
        {
            _context = context;
        }

        // Get all user events
        public async Task<List<Event>> GetAllUserEventsAsync(int userId)
        {
            return await _context.Events
                .Where(e => e.EventUsers.Any(eu => eu.UserId == userId))
                .Include(e => e.Calendar)
                .Include(e => e.EventUsers)
                .Include(e => e.Invitations)
                .ToListAsync();
        }

        // Hent et enkelt event
        public async Task<Event?> GetEventByIdAsync(int eventId)
        {
            return await _context.Events
                .Include(e => e.Calendar)
                .Include(e => e.EventUsers)
                .Include(e => e.Invitations)
                .FirstOrDefaultAsync(e => e.EventId == eventId);
        }

        // Opret et nyt event
        public async Task AddEventAsync(Event eevent)
        {
            if (eevent == null)
                throw new ArgumentNullException(nameof(eevent));

            _context.Events.Add(eevent);
            await _context.SaveChangesAsync();
        }

        // Opdater et event
        public async Task UpdateEventAsync(Event updatedEvent)
        {
            var existingEvent = await _context.Events.FindAsync(updatedEvent.EventId);
            if (existingEvent == null)
                throw new KeyNotFoundException($"Event with ID {updatedEvent.EventId} not found.");

            existingEvent.EventTitle = updatedEvent.EventTitle;
            existingEvent.EventDescription = updatedEvent.EventDescription;
            existingEvent.EventStart = updatedEvent.EventStart;
            existingEvent.EventEnd = updatedEvent.EventEnd;
            existingEvent.CalendarId = updatedEvent.CalendarId;

            await _context.SaveChangesAsync();
        }

        // Slet et event
        public async Task DeleteEventAsync(int eventId)
        {
            var eventToDelete = await _context.Events
                .Include(e => e.EventUsers)
                .Include(e => e.Invitations)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventToDelete == null)
                throw new KeyNotFoundException($"Event with ID {eventId} not found.");

            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByUserIdAsync(int userId)
        {
            return await _context.Events
                .Where(e => e.EventUsers.Any(eu => eu.UserId == userId))
                .Include(e => e.Calendar)
                .Include(e => e.EventUsers)
                .Include(e => e.Invitations)
                .ToListAsync();
        }
    }
}
