using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhostOfJoe.Models.VoteDatabase;

[Table("race")]
public partial class Race
{
    [Key]
    [Column("race_id")]
    public int RaceId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [InverseProperty("Race")]
    public virtual ICollection<Representitive> Representitives { get; set; } = new List<Representitive>();
}
