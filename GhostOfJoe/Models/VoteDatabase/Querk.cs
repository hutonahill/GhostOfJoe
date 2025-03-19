using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhostOfJoe.Models.VoteDatabase;

[Table("querks")]
public partial class Querk
{
    [Key]
    [Column("querk_id")]
    public int QuerkId { get; set; }

    [Column("name")]
    public int Name { get; set; }
}
