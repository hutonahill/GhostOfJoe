using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhostOfJoe.Models.VoteDatabase;

[Table("party")]
public partial class Party
{
    [Key]
    [Column("party_id")]
    public int PartyId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [InverseProperty("Party")]
    public virtual ICollection<Representitive> Representitives { get; set; } = new List<Representitive>();
}
