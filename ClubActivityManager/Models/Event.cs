using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubActivityManager.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime DateTime { get; set; } = DateTime.UtcNow;

        public string Location { get; set; }

        [ForeignKey("Creator")]
        public string CreatedBy { get; set; } 
        public ApplicationUser Creator { get; set; }

        // Navigation
        public ICollection<EventRegistration>? EventRegistrations { get; set; }
        public ICollection<ResourceReservation>? ResourceReservations { get; set; }
    }

}
