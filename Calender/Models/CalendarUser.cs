using System.Text.Json.Serialization;

namespace Calender.Models
{
    public class CalendarUser
    {
        public int CalendarId { get; set; }
        [JsonIgnore]
        public virtual Calendar? Calendar { get; set; }

        public int UserId { get; set; }
        [JsonIgnore]
        public User? User { get; set; }

        public enum PermissionLevel
        {
            User = 0,
            Moderator =1,
            Owner =2
        }
        public PermissionLevel Permissions { get; set; }

        public bool CanEdit => Permissions == PermissionLevel.Moderator || Permissions == PermissionLevel.Owner;
        public bool CanInvite => Permissions == PermissionLevel.Owner;
        public bool IsOwner => Permissions == PermissionLevel.Owner;
    }
}
