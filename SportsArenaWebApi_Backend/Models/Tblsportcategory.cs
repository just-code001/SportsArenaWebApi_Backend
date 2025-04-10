using System;
using System.Collections.Generic;

namespace SportsArenaWebApi_Backend.Models;

public partial class Tblsportcategory
{
    public int CategoryId { get; set; }

    public string Categoryname { get; set; } = null!;

    public virtual ICollection<Tblvenue> Tblvenues { get; set; } = new List<Tblvenue>();
}
