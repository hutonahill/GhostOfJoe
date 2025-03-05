using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class Gender
{
    public int Name { get; set; }

    public int GenderId { get; set; }

    public virtual ICollection<Representitive> Representitives { get; set; } = new List<Representitive>();
}
