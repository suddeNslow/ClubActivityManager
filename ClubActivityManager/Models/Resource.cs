using System.ComponentModel.DataAnnotations;

namespace ClubActivityManager.Models
{
    public class Resource
    {
        public int ResourceId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Type { get; set; }

        public string Availability { get; set; } // e.g. "available", "reserved"

        // Navigation
        public ICollection<ResourceReservation>? ResourceReservations { get; set; }
    }

}
