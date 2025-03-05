using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class Score
{
    public int ScoreId { get; set; }

    public byte[] Value { get; set; } = null!;

    public int CategoryId { get; set; }

    public int UserId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
