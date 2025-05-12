using System.ComponentModel.DataAnnotations;

namespace ClubActivityManager.Models
{
    public class EventWithReservationViewModel
    {
        // Event
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime DateTime { get; set; } = DateTime.UtcNow;

        public string Location { get; set; }

        // Resources (multiple)
        [Required]
        public List<int> ResourceIds { get; set; } = new();

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public List<Resource>? AvailableResources { get; set; }
    }
}
