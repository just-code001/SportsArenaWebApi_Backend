namespace SportsArenaWebApi_Backend.DTOs
{
    public class PaymentMethodDto
    {
        public string Method { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}
