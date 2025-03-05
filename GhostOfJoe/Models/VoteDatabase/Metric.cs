using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("metric")]
public partial class Metric
{
    [Key]
    [Column("metric_id")]
    public int MetricId { get; set; }

    [Column("name")]
    public int Name { get; set; }
}
