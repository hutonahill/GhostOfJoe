using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("users")]
public partial class Users
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("discordUser_id")]
    public ulong DiscordUserId { get; set; }

    [Column("server_id")]
    public ulong ServerId { get; set; }

    [Column("preferred_score_set_id")]
    public int? PreferredScoreSetId { get; set; }

    [Column("preferred_weight_set_id")]
    public int? PreferredWeightSetId { get; set; }

    [ForeignKey("PreferredScoreSetId")]
    [InverseProperty("Users")]
    public virtual MetricScoreSet? PreferredScoreSet { get; set; }

    [ForeignKey("PreferredWeightSetId")]
    [InverseProperty("Users")]
    public virtual MetricWeightSet? PreferredWeightSet { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();

    [ForeignKey("ServerId")]
    [InverseProperty("Users")]
    public virtual Servers Servers { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<Titles> Titles { get; set; } = new List<Titles>();
}
