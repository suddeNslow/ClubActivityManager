namespace ClubActivityManager.Models
{
    public class EventRegistration
    {
        public int EventRegistrationId { get; set; }

        public string UserId { get; set; }  
        public ApplicationUser User { get; set; }  

        public int EventId { get; set; }
        public Event Event { get; set; }

        public DateTime RegistrationDate { get; set; }
    }

}
