using System;
using System.Collections.Generic;

namespace SportsArenaWebApi_Backend.Models;

public partial class Tblbooking
{
    public int BookingId { get; set; }

    public int SlotId { get; set; }

    public int UserId { get; set; }

    public decimal PayableAmount { get; set; }

    public bool PaymentPaid { get; set; }

    public string BookingStatus { get; set; } = null!;

    public DateTime BookingDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Tblvenueslot Slot { get; set; } = null!;

    public virtual ICollection<Tblpayment> Tblpayments { get; set; } = new List<Tblpayment>();

    public virtual Tbluser User { get; set; } = null!;
}
