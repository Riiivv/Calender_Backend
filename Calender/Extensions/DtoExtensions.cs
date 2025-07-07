using Calender.DTO;
using Calender.Models;

namespace Calender.Extensions
{
    public static class DtoExtensions
    {
        public static UserDto ToDTO(this User user, bool includeId)
        {
            return new UserDto
            {
                Username = user.Username,
                UserId = includeId ? user.UserId : null
            };
        }
    }
}
