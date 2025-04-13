namespace ClubActivityManager.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string Method { get; set; } // e.g. "cash", "card"
    }

}
