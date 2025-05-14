using Microsoft.EntityFrameworkCore;

namespace Calender.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<CalendarInvitation> CalendarInvitations { get; set; }
        public DbSet<CalendarUser> CalendarUsers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventInvitation> EventInvitations { get; set; }
        public DbSet<EventUser> EventUsers { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<CalendarInvitation>()
                .HasKey(ci => ci.InvitationId);

            // User -> Calendar (En-til-mange)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Calendars)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.Userid)
                .OnDelete(DeleteBehavior.Cascade);

            // Calendar -> Event (En-til-mange)
            modelBuilder.Entity<Calendar>()
                .HasMany(c => c.Events)
                .WithOne(e => e.Calendar)
                .HasForeignKey(e => e.CalendarId)
                .OnDelete(DeleteBehavior.Cascade);

            // User (Sender) -> CalendarInvitation (En-til-mange)
            modelBuilder.Entity<User>()
                .HasMany(u => u.SentInvitations)
                .WithOne(ci => ci.Sender)
                .HasForeignKey(ci => ci.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // User (Recipient) -> CalendarInvitation (En-til-mange)
            modelBuilder.Entity<User>()
                .HasMany(u => u.RecievedInvitations)
                .WithOne(ci => ci.Recipient)
                .HasForeignKey(ci => ci.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Calendar -> CalendarInvitation (En-til-mange)
            modelBuilder.Entity<Calendar>()
                .HasMany(c => c.Invitations)
                .WithOne(ci => ci.Calendar)
                .HasForeignKey(ci => ci.CalendarId)
                .OnDelete(DeleteBehavior.Cascade);

            // Event -> EventInvitation (En-til-mange)
            modelBuilder.Entity<Event>()
                .HasMany(e => e.Invitations)
                .WithOne(ei => ei.Event)
                .HasForeignKey(ei => ei.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Mange-til-mange mellem Event og User via EventUser
            modelBuilder.Entity<EventUser>()
                .HasKey(eu => new { eu.EventId, eu.UserId });

            modelBuilder.Entity<EventUser>()
                .HasOne(eu => eu.Event)
                .WithMany(e => e.EventUsers)
                .HasForeignKey(eu => eu.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventUser>()
                .HasOne(eu => eu.User)
                .WithMany(u => u.EventUsers)
                .HasForeignKey(eu => eu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mange-til-mange mellem Calendar og User via CalendarUser
            modelBuilder.Entity<CalendarUser>()
                .HasKey(cu => new { cu.CalendarId, cu.UserId });

            modelBuilder.Entity<CalendarUser>()
                .HasOne(cu => cu.Calendar)
                .WithMany(c => c.CalendarUsers)
                .HasForeignKey(cu => cu.CalendarId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CalendarUser>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.CalendarUsers)
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // EventInvitation relationer
            modelBuilder.Entity<EventInvitation>()
                .HasKey(ei => ei.InvitationId);

            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.Sender)
                .WithMany(u => u.SentEventInvitations)
                .HasForeignKey(ei => ei.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.Recipient)
                .WithMany(u => u.RecievedEventInvitations)
                .HasForeignKey(ei => ei.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.Event)
                .WithMany(e => e.Invitations)
                .HasForeignKey(ei => ei.EventId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
