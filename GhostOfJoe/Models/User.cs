using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("users")]
public partial class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("discordUser_id")]
    public int DiscordUserId { get; set; }

    [Column("server_id")]
    public int ServerId { get; set; }

    [Column("prefered_score_set_id")]
    public int? PreferedScoreSetId { get; set; }

    [Column("prefered_weight_set_id")]
    public int? PreferedWeightSetId { get; set; }

    [ForeignKey("PreferedScoreSetId")]
    [InverseProperty("Users")]
    public virtual MetricScoreSet? PreferedScoreSet { get; set; }

    [ForeignKey("PreferedWeightSetId")]
    [InverseProperty("Users")]
    public virtual MetricWeightSet? PreferedWeightSet { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();

    [ForeignKey("ServerId")]
    [InverseProperty("Users")]
    public virtual Server Server { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<Title> Titles { get; set; } = new List<Title>();
}
