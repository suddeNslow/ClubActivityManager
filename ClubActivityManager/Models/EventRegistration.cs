﻿namespace ClubActivityManager.Models
{
    public class EventRegistration
    {
        public int EventRegistrationId { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public DateTime RegistrationDate { get; set; }
    }

}
