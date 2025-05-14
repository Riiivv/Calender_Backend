using Calender.Interface;
using Calender.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calender.Repositories
{
    public class EventUserRepo : IEventUser
    {
        private readonly DatabaseContext _context;

        public EventUserRepo(DatabaseContext context)
        {
            _context = context;
        }

        // Hent alle EventUsers
        public async Task<List<EventUser>> GetAllEventUsersAsync()
        {
            return await _context.EventUsers
                .Include(eu => eu.User)
                .Include(eu => eu.Event)
                .ToListAsync();
        }

        // Hent en enkelt EventUser
        public async Task<EventUser?> GetEventUserAsync(int eventId, int userId)
        {
            return await _context.EventUsers
                .Include(eu => eu.User)
                .Include(eu => eu.Event)
                .FirstOrDefaultAsync(eu => eu.EventId == eventId && eu.UserId == userId);
        }

        // Opret en EventUser
        public async Task AddEventUserAsync(EventUser eventUser)
        {
            _context.EventUsers.Add(eventUser);
            await _context.SaveChangesAsync();
        }

        // Opdater en EventUser (kun permissions)
        public async Task UpdateEventUserAsync(EventUser eventUser)
        {
            var existingEventUser = await _context.EventUsers
                .FirstOrDefaultAsync(eu => eu.EventId == eventUser.EventId && eu.UserId == eventUser.UserId);

            if (existingEventUser == null)
                throw new KeyNotFoundException("EventUser not found.");

            // Opdater kun permissions
            existingEventUser.Permissions = eventUser.Permissions;

            await _context.SaveChangesAsync();
        }

        // Slet en EventUser
        public async Task DeleteEventUserAsync(int eventId, int userId)
        {
            var eventUser = await _context.EventUsers
                .FirstOrDefaultAsync(eu => eu.EventId == eventId && eu.UserId == userId);

            if (eventUser != null)
            {
                _context.EventUsers.Remove(eventUser);
                await _context.SaveChangesAsync();
            }
        }
    }
}
