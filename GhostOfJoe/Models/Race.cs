using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class Race
{
    public int RaceId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Representitive> Representitives { get; set; } = new List<Representitive>();
}
