using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

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

    [InverseProperty("Game")]
    public virtual ICollection<Categories> Categories { get; set; } = new List<Categories>();

    [ForeignKey("ServerId")]
    [InverseProperty("Games")]
    public virtual Servers Servers { get; set; } = null!;
}
