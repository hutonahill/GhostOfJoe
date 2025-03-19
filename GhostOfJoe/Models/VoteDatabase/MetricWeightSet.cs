using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhostOfJoe.Models.VoteDatabase;

[Table("metric_weight_set")]
public partial class MetricWeightSet
{
    [Key]
    [Column("weight_set_id")]
    public int WeightSetId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [InverseProperty("PreferredWeightSet")]
    public virtual ICollection<Users> Users { get; set; } = new List<Users>();
}
