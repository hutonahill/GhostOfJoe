using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class Office
{
    public int OfficeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Representitive> Representitives { get; set; } = new List<Representitive>();
}
