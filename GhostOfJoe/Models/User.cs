using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class User
{
    public int UserId { get; set; }

    public int DiscordUserId { get; set; }

    public int ServerId { get; set; }

    public int? PreferedScoreSetId { get; set; }

    public int? PreferedWeightSetId { get; set; }

    public virtual MetricScoreSet? PreferedScoreSet { get; set; }

    public virtual MetricWeightSet? PreferedWeightSet { get; set; }

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();

    public virtual Server Server { get; set; } = null!;

    public virtual ICollection<Title> Titles { get; set; } = new List<Title>();
}
