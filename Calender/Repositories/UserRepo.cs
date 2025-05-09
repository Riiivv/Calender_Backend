using Calender.Interface;
using Calender.Models;

namespace Calender.Repositories
{
    public class UserRepo : IUser
    {
        DatabaseContext ctx;
        public UserRepo (DatabaseContext Context)
        {
            ctx = Context;
        }

        public List<User> GetUsers()
        {
            return ctx.Users.ToList();
        }
    }
}
