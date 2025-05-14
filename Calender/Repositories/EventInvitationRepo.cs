using Calender.Interface;
using Calender.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calender.Repositories
{
    public class EventInvitationRepo : IEventInvitation
    {
        private readonly DatabaseContext _context;

        public EventInvitationRepo(DatabaseContext context)
        {
            _context = context;
        }

        // Hent alle Event Invitations
        public async Task<List<EventInvitation>> GetAllEventInvitationsAsync()
        {
            return await _context.EventInvitations
                .Include(ei => ei.Sender)
                .Include(ei => ei.Recipient)
                .Include(ei => ei.Event)
                .ToListAsync();
        }

        // Hent en enkelt Event Invitation
        public async Task<EventInvitation?> GetEventInvitationAsync(int eventId, int recipientId)
        {
            return await _context.EventInvitations
                .Include(ei => ei.Sender)
                .Include(ei => ei.Recipient)
                .Include(ei => ei.Event)
                .FirstOrDefaultAsync(ei => ei.EventId == eventId && ei.RecipientId == recipientId);
        }

        // Opret en Event Invitation
        public async Task AddEventInvitationAsync(EventInvitation invitation)
        {
            _context.EventInvitations.Add(invitation);
            await _context.SaveChangesAsync();
        }

        // Opdater en Event Invitation
        public async Task UpdateEventInvitationAsync(EventInvitation invitation)
        {
            var existingInvitation = await _context.EventInvitations
                .FirstOrDefaultAsync(ei => ei.EventId == invitation.EventId && ei.RecipientId == invitation.RecipientId);

            if (existingInvitation == null)
                throw new KeyNotFoundException("EventInvitation not found.");

            existingInvitation.SenderId = invitation.SenderId;
            existingInvitation.RecipientId = invitation.RecipientId;
            existingInvitation.EventId = invitation.EventId;

            await _context.SaveChangesAsync();
        }

        // Slet en Event Invitation
        public async Task DeleteEventInvitationAsync(int eventId, int recipientId)
        {
            var invitation = await _context.EventInvitations
                .FirstOrDefaultAsync(ei => ei.EventId == eventId && ei.RecipientId == recipientId);

            if (invitation != null)
            {
                _context.EventInvitations.Remove(invitation);
                await _context.SaveChangesAsync();
            }
        }
    }
}
