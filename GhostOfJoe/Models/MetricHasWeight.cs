using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class MetricHasWeight
{
    public int MetricId { get; set; }

    public int WeightSetId { get; set; }

    public int Weight { get; set; }

    public virtual Metric Metric { get; set; } = null!;

    public virtual MetricWeightSet WeightSet { get; set; } = null!;
}
