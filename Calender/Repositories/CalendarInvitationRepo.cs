using Calender.Interface;
using Calender.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calender.Repositories
{
    public class CalendarInvitationRepo : ICalendarInvitation
    {
        private readonly DatabaseContext _context;

        public CalendarInvitationRepo(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<CalendarInvitation>> GetAllCalendarInvitationsAsync()
        {
            return await _context.CalendarInvitations
                .Include(ci => ci.Sender)
                .Include(ci => ci.Recipient)
                .Include(ci => ci.Calendar)
                .ToListAsync();
        }

        public async Task<CalendarInvitation?> GetCalendarInvitationAsync(int invitationId)
        {
            return await _context.CalendarInvitations
                .Include(ci => ci.Sender)
                .Include(ci => ci.Recipient)
                .Include(ci => ci.Calendar)
                .FirstOrDefaultAsync(ci => ci.InvitationId == invitationId);
        }

        public async Task AddCalendarInvitationAsync(CalendarInvitation invitation)
        {
            _context.CalendarInvitations.Add(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCalendarInvitationAsync(CalendarInvitation invitation)
        {
            var existingInvitation = await _context.CalendarInvitations.FindAsync(invitation.InvitationId);
            if (existingInvitation == null)
                throw new KeyNotFoundException($"Invitation with ID {invitation.InvitationId} not found.");

            existingInvitation.SenderId = invitation.SenderId;
            existingInvitation.RecipientId = invitation.RecipientId;
            existingInvitation.CalendarId = invitation.CalendarId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCalendarInvitationAsync(int invitationId)
        {
            var invitation = await _context.CalendarInvitations.FindAsync(invitationId);
            if (invitation != null)
            {
                _context.CalendarInvitations.Remove(invitation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User?> GetRecipientByCalendarInvitationAsync(int invitationId)
        {
            var invitation = await _context.CalendarInvitations
                .Include(ci => ci.Recipient)
                .FirstOrDefaultAsync(ci => ci.InvitationId == invitationId);

            return invitation?.Recipient;
        }

        public async Task<User?> GetSenderByCalendarInvitationAsync(int invitationId)
        {
            var invitation = await _context.CalendarInvitations
                .Include(ci => ci.Sender)
                .FirstOrDefaultAsync(ci => ci.InvitationId == invitationId);

            return invitation?.Sender;
        }
    }
}
