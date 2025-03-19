using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhostOfJoe.Models.GameSystem;

[Table("categories")]
public partial class Categories
{
    [Key]
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("game_id")]
    public int GameId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("unit")]
    public string Unit { get; set; } = null!;

    [Column("higherBetter")]
    public bool HigherBetter { get; set; }

    [ForeignKey(nameof(GameId))]
    [InverseProperty(nameof(GameSystem.Game.Categories))]
    public virtual Game Game { get; set; } = null!;

    [InverseProperty(nameof(Score.Categories))]
    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
}
