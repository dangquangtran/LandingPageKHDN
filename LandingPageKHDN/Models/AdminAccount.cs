using System;
using System.Collections.Generic;

namespace LandingPageKHDN.Models;

public partial class AdminAccount
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ActionLog> ActionLogs { get; set; } = new List<ActionLog>();
}
