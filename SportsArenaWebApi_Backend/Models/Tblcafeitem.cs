using System;
using System.Collections.Generic;

namespace SportsArenaWebApi_Backend.Models;

public partial class Tblcafeitem
{
    public int ItemId { get; set; }

    public string ItemName { get; set; } = null!;

    public decimal Price { get; set; }

    public int AvailableQuantity { get; set; }

    public virtual ICollection<Tblcafeorderdetail> Tblcafeorderdetails { get; set; } = new List<Tblcafeorderdetail>();
}
