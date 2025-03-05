using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class Party
{
    public int PartyId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Representitive> Representitives { get; set; } = new List<Representitive>();
}
