using System;
using System.Collections.Generic;

namespace SportsArenaWebApi_Backend.Models;

public partial class Tblinquiry
{
    public int InquiryId { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? InquiryDate { get; set; }

    public virtual Tbluser User { get; set; } = null!;
}
