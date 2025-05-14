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

        // Hent alle events
        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events
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
            _context.Events.Add(eevent);
            await _context.SaveChangesAsync();
        }

        // Opdater et event
        public async Task UpdateEventAsync(Event eventUpdate)
        {
            var existingEvent = await _context.Events.FindAsync(eventUpdate.EventId);
            if (existingEvent == null)
                throw new KeyNotFoundException("Event not found.");

            existingEvent.EventTitle = eventUpdate.EventTitle;
            existingEvent.EventDescription = eventUpdate.EventDescription;
            existingEvent.EventStart = eventUpdate.EventStart;
            existingEvent.EventEnd = eventUpdate.EventEnd;
            existingEvent.CalendarId = eventUpdate.CalendarId;

            await _context.SaveChangesAsync();
        }

        // Slet et event
        public async Task DeleteEventAsync(int eventId)
        {
            var eventToDelete = await _context.Events.FindAsync(eventId);
            if (eventToDelete != null)
            {
                _context.Events.Remove(eventToDelete);
                await _context.SaveChangesAsync();
            }
        }
    }
}
