using Calender.Models;

namespace Calender.DTO
{
    public class CalendarDTO
    {
        public int CalendarId { get; set; }
        public string CalendarName { get; set; } = string.Empty;
        public int UserId { get; set; } // Owner's user ID
        public CalendarUser.PermissionLevel UserPermission { get; set; } // Current user's permission level
        // public int MemberCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Computed properties for frontend convenience
        public bool CanEdit => UserPermission == CalendarUser.PermissionLevel.Owner || 
                               UserPermission == CalendarUser.PermissionLevel.Moderator;
        public bool CanInvite => UserPermission == CalendarUser.PermissionLevel.Owner;
        public bool IsOwner => UserPermission == CalendarUser.PermissionLevel.Owner;
        public string PermissionName => UserPermission.ToString();
    }
}