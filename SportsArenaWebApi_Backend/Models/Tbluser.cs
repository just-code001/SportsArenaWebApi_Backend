using System;
using System.Collections.Generic;

namespace SportsArenaWebApi_Backend.Models;

public partial class Tbluser
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Contact { get; set; } = null!;

    public virtual Tblrole Role { get; set; } = null!;

    public virtual ICollection<Tblblog> Tblblogs { get; set; } = new List<Tblblog>();

    public virtual ICollection<Tblbooking> Tblbookings { get; set; } = new List<Tblbooking>();

    public virtual ICollection<Tblcafeorder> Tblcafeorders { get; set; } = new List<Tblcafeorder>();

    public virtual ICollection<Tblinquiry> Tblinquiries { get; set; } = new List<Tblinquiry>();

    public virtual ICollection<Tblreview> Tblreviews { get; set; } = new List<Tblreview>();

    public virtual ICollection<Tblvenue> Tblvenues { get; set; } = new List<Tblvenue>();
}
