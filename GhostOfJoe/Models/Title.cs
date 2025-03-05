using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class Title
{
    public int TitleId { get; set; }

    public string Title1 { get; set; } = null!;

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
