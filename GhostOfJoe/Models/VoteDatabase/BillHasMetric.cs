using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Keyless]
[Table("bill_has_metric")]
public partial class BillHasMetric
{
    [Column("score_set_id")]
    public int ScoreSetId { get; set; }

    [Column("bill_id")]
    public int BillId { get; set; }

    [Column("metric_id")]
    public int MetricId { get; set; }

    [Column("score")]
    public int Score { get; set; }

    [ForeignKey("BillId")]
    public virtual Bill Bill { get; set; } = null!;

    [ForeignKey("MetricId")]
    public virtual Metric Metric { get; set; } = null!;

    [ForeignKey("ScoreSetId")]
    public virtual MetricScoreSet ScoreSet { get; set; } = null!;
}
