using System.ComponentModel.DataAnnotations;

namespace Calender.DTO
{
    public class CalendarDTO
    {
        [Required]
        public int CalendarId { get; set; }
        [Required]
        public string CalendarName { get; set; }

        public int Userid { get; set; }
    }
}