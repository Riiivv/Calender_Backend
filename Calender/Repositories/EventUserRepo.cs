using Calender.Interface;
using Calender.Models;

namespace Calender.Repositories
{
    public class EventUserRepo : IEventUser
    {
        DatabaseContext ctx;

        public EventUserRepo (DatabaseContext context)
        {
            ctx = context;
        }

        public List<EventUser> GetEventUsers()
        {
            return ctx.EventUsers.ToList();
        } 
        public EventUser GetEventUser(int eventId)
        {
            return ctx.EventUsers.Where(e => e.EventId == eventId).FirstOrDefault();
        }
    }
}