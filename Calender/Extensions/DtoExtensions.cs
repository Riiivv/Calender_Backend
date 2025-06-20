using Calender.DTO;
using Calender.Models;

namespace Calender.Extensions
{
    public static class DtoExtensions
    {
        public static UserDto ToDTO(this User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username
            };
        }
    }
}
