using System;
using System.Collections.Generic;

namespace SportsArenaWebApi_Backend.Models;

public partial class Tblvenueslot
{
    public int SlotId { get; set; }

    public int VenueId { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsBooked { get; set; }

    public virtual ICollection<Tblbooking> Tblbookings { get; set; } = new List<Tblbooking>();

    public virtual Tblvenue Venue { get; set; } = null!;
}
