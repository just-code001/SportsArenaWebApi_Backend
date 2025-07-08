using System.ComponentModel.DataAnnotations;

namespace SportsArenaWebApi_Backend.DTOs
{
    public class CreateMultiplePaymentsDto
    {
        [Required]
        public List<int> BookingIds { get; set; } = new List<int>();

        [Required]
        public string TransactionId { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string PaymentStatus { get; set; } = null!;

        public string? PaymentMethod { get; set; } = "Razorpay";

        public string? PaymentGatewayResponse { get; set; }
    }
}
