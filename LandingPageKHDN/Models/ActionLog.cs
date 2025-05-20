using System;
using System.Collections.Generic;

namespace LandingPageKHDN.Models;

public partial class ActionLog
{
    public int Id { get; set; }

    public int? AdminId { get; set; }

    public string Action { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual AdminAccount Admin { get; set; } = null!;
}
