using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models.VoteDatabase;

[Keyless]
[Table("metric_has_weights")]
public partial class MetricHasWeight
{
    [Column("metric_id")]
    public int MetricId { get; set; }

    [Column("weight_set_id")]
    public int WeightSetId { get; set; }

    [Column("weight")]
    public int Weight { get; set; }

    [ForeignKey("MetricId")]
    public virtual Metric Metric { get; set; } = null!;

    [ForeignKey("WeightSetId")]
    public virtual MetricWeightSet WeightSet { get; set; } = null!;
}
