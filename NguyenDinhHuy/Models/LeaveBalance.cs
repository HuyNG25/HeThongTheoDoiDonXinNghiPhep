using System;
using System.Collections.Generic;

namespace NguyenDinhHuy.Models;

public partial class LeaveBalance
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int Year { get; set; }

    public int? TotalAllowedDays { get; set; }

    public int? UsedDays { get; set; }

    public virtual User? User { get; set; }
}
