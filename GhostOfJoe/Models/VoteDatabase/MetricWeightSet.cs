using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("metric_weight_set")]
public partial class MetricWeightSet
{
    [Key]
    [Column("weight_set_id")]
    public int WeightSetId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [InverseProperty("PreferedWeightSet")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
