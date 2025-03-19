
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GhostOfJoe.Models.GameSystem;
using GhostOfJoe.Models.VoteDatabase;


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
    
    
    [ForeignKey(nameof(PreferredScoreSetId))]
    [InverseProperty(nameof(VoteDatabase.MetricScoreSet.Users))]
    public virtual MetricScoreSet? PreferredScoreSet { get; set; }

    [ForeignKey(nameof(PreferredWeightSetId))]
    [InverseProperty(nameof(VoteDatabase.MetricWeightSet.Users))]
    public virtual MetricWeightSet? PreferredWeightSet { get; set; }

    [InverseProperty(nameof(GameSystem.Score.Users))]
    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
    
    [ForeignKey(nameof(ServerId))]
    [InverseProperty(nameof(Models.Servers.Users))]
    public virtual Servers Servers { get; set; } = null!;

    [InverseProperty(nameof(Models.Titles.Users))]
    public virtual ICollection<Titles> Titles { get; set; } = new List<Titles>();
}
