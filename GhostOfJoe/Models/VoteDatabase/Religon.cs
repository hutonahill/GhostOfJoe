using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhostOfJoe.Models.VoteDatabase;

[Table("religon")]
public partial class Religon
{
    [Key]
    [Column("religon_id")]
    public int ReligonId { get; set; }

    [Column("name")]
    public int? Name { get; set; }

    [InverseProperty("Religon")]
    public virtual ICollection<Representitive> Representitives { get; set; } = new List<Representitive>();
}
