namespace ClubActivityManager.Models
{
    public class ResourceReservation
    {
        public int ResourceReservationId { get; set; }

        public int ResourceId { get; set; }
        public Resource Resource { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }

}
