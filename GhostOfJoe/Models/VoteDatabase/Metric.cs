using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhostOfJoe.Models.VoteDatabase;

[Table("metric")]
public partial class Metric
{
    [Key]
    [Column("metric_id")]
    public int MetricId { get; set; }

    [Column("name")]
    public int Name { get; set; }
}
