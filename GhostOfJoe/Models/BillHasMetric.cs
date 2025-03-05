using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class BillHasMetric
{
    public int ScoreSetId { get; set; }

    public int BillId { get; set; }

    public int MetricId { get; set; }

    public int Score { get; set; }

    public virtual Bill Bill { get; set; } = null!;

    public virtual Metric Metric { get; set; } = null!;

    public virtual MetricScoreSet ScoreSet { get; set; } = null!;
}
