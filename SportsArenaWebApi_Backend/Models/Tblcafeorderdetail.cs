using System;
using System.Collections.Generic;

namespace SportsArenaWebApi_Backend.Models;

public partial class Tblcafeorderdetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public virtual Tblcafeitem Item { get; set; } = null!;

    public virtual Tblcafeorder Order { get; set; } = null!;
}
