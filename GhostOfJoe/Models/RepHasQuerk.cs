using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class RepHasQuerk
{
    public int RepId { get; set; }

    public int QuerkId { get; set; }

    public virtual Querk Querk { get; set; } = null!;

    public virtual Representitive Rep { get; set; } = null!;
}
