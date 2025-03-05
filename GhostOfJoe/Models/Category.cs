using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public int GameId { get; set; }

    public string Name { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public int HigherBetter { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
}
