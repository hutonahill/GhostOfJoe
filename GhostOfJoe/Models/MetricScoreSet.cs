using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class MetricScoreSet
{
    public int ScoreSetId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
