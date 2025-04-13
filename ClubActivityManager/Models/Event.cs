using System.ComponentModel.DataAnnotations;

namespace ClubActivityManager.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime DateTime { get; set; }

        public string Location { get; set; }

        public int CreatedBy { get; set; }
        public Member Creator { get; set; }

        // Navigation
        public ICollection<EventRegistration>? EventRegistrations { get; set; }
        public ICollection<ResourceReservation>? ResourceReservations { get; set; }
    }

}
