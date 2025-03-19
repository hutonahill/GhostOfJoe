using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhostOfJoe.Models.VoteDatabase;

[Table("vote_type")]
public partial class VoteType
{
    [Key]
    [Column("vote_type_id")]
    public int VoteTypeId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;
}
