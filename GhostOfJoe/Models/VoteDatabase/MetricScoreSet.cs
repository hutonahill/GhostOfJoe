using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("metric_score_set")]
public partial class MetricScoreSet
{
    [Key]
    [Column("score_set_id")]
    public int ScoreSetId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [InverseProperty("PreferedScoreSet")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
