using Microsoft.EntityFrameworkCore;

namespace Calender.Models
{
    public class DatabaseContext :DbContext
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
    }
}
