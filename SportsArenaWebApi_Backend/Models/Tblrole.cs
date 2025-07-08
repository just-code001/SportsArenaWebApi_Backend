using System;
using System.Collections.Generic;

namespace SportsArenaWebApi_Backend.Models;

public partial class Tblrole
{
    public int RoleId { get; set; }

    public string Rolename { get; set; } = null!;

    public virtual ICollection<Tbluser> Tblusers { get; set; } = new List<Tbluser>();
}
