using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class Religon
{
    public int ReligonId { get; set; }

    public int? Name { get; set; }

    public virtual ICollection<Representitive> Representitives { get; set; } = new List<Representitive>();
}
