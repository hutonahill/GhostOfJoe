using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models.VoteDatabase;

[Keyless]
[Table("rep_has_querk")]
public partial class RepHasQuerk
{
    [Column("rep_id")]
    public int RepId { get; set; }

    [Column("querk_id")]
    public int QuerkId { get; set; }

    [ForeignKey("QuerkId")]
    public virtual Querk Querk { get; set; } = null!;

    [ForeignKey("RepId")]
    public virtual Representitive Rep { get; set; } = null!;
}
