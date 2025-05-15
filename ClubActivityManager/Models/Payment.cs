using System.ComponentModel.DataAnnotations;

namespace ClubActivityManager.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        [Required(ErrorMessage = "Member is required.")]
        public string MemberId { get; set; }
        public ApplicationUser Member { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment date is required.")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Payment method is required.")]
        public string Method { get; set; } 
    }
}
