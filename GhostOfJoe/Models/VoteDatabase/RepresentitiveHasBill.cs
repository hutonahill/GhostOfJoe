using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Keyless]
[Table("representitive_has_bills")]
public partial class RepresentitiveHasBill
{
    [Column("rep_id")]
    public int RepId { get; set; }

    [Column("vote_type_id")]
    public int VoteTypeId { get; set; }

    [Column("bill_id")]
    public int BillId { get; set; }

    [ForeignKey("BillId")]
    public virtual Bill Bill { get; set; } = null!;

    [ForeignKey("RepId")]
    public virtual Representitive Rep { get; set; } = null!;

    [ForeignKey("VoteTypeId")]
    public virtual VoteType VoteType { get; set; } = null!;
}
