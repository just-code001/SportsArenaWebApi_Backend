using System;
using System.Collections.Generic;

namespace SportsArenaWebApi_Backend.Models;

public partial class Tblcafeorder
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime? OrderDate { get; set; }

    public virtual ICollection<Tblcafeorderdetail> Tblcafeorderdetails { get; set; } = new List<Tblcafeorderdetail>();

    public virtual Tbluser User { get; set; } = null!;
}
