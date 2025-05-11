using ClubActivityManager.Models;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string Role { get; set; } // e.g., "admin", "member"

    // Navigation
    public ICollection<Event>? CreatedEvents { get; set; }
    public ICollection<EventRegistration>? EventRegistrations { get; set; }
    public ICollection<Payment>? Payments { get; set; }
}
