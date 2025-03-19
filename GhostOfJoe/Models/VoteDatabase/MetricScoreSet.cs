using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhostOfJoe.Models.VoteDatabase;

[Table("metric_score_set")]
public sealed partial class MetricScoreSet
{
    [Key]
    [Column("score_set_id")]
    public int ScoreSetId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;
    
    
    [InverseProperty(nameof(Models.Users.PreferredScoreSet))]
    public ICollection<Users> Users { get; set; } = new List<Users>();
}
