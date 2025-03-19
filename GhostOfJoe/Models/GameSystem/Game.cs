using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhostOfJoe.Models.GameSystem;

[Table("games")]
public partial class Game
{
    [Key]
    [Column("game_id")]
    public int GameId { get; set; }

    [Column("server_id")]
    public ulong ServerId { get; set; }

    [Column("title")]
    public string Title { get; set; } = null!;
    
    [InverseProperty(nameof(GameSystem.Categories.Game))]
    public virtual ICollection<Categories> Categories { get; set; } = new List<Categories>();

    [ForeignKey(nameof(ServerId))]
    [InverseProperty(nameof(Servers.Games))]
    public virtual Servers Servers { get; set; } = null!;
}
