using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClubActivityManager.Models
{
    public class Member
    {
        public int MemberId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, PasswordPropertyText]
        public string Password { get; set; }

        [Phone]
        public string Phone { get; set; }

        [Required]
        public string Role { get; set; } // "member", "admin"

        // Navigation
        public ICollection<Event>? CreatedEvents { get; set; }
        public ICollection<EventRegistration>? EventRegistrations { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }


}
