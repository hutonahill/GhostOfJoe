using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class RepresentitiveHasBill
{
    public int RepId { get; set; }

    public int VoteTypeId { get; set; }

    public int BillId { get; set; }

    public virtual Bill Bill { get; set; } = null!;

    public virtual Representitive Rep { get; set; } = null!;

    public virtual VoteType VoteType { get; set; } = null!;
}
