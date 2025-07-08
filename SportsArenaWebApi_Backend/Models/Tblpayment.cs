using System;
using System.Collections.Generic;

namespace SportsArenaWebApi_Backend.Models;

public partial class Tblpayment
{
    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    public string TransactionId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentGatewayResponse { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Tblbooking Booking { get; set; } = null!;
}
