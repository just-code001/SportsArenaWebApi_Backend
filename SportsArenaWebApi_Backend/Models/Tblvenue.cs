using System;
using System.Collections.Generic;

namespace SportsArenaWebApi_Backend.Models;

public partial class Tblvenue
{
    public int VenueId { get; set; }

    public int ProviderId { get; set; }

    public int CategoryId { get; set; }

    public string Venuename { get; set; } = null!;

    public string Location { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Capacity { get; set; }

    public decimal Priceperhour { get; set; }

    public bool IsActive { get; set; }

    public string VenueImage { get; set; } = null!;

    public virtual Tblsportcategory Category { get; set; } = null!;

    public virtual Tbluser Provider { get; set; } = null!;

    public virtual ICollection<Tblreview> Tblreviews { get; set; } = new List<Tblreview>();

    public virtual ICollection<Tblvenueslot> Tblvenueslots { get; set; } = new List<Tblvenueslot>();
}
