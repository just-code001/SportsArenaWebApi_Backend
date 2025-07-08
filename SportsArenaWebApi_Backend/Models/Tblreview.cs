using System;
using System.Collections.Generic;

namespace SportsArenaWebApi_Backend.Models;

public partial class Tblreview
{
    public int ReviewId { get; set; }

    public int VenueId { get; set; }

    public int UserId { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; } = null!;

    public virtual Tbluser User { get; set; } = null!;

    public virtual Tblvenue Venue { get; set; } = null!;
}
