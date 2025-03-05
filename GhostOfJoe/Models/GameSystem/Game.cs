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
    public int ServerId { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [InverseProperty("Game")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    [ForeignKey("ServerId")]
    [InverseProperty("Games")]
    public virtual Server Server { get; set; } = null!;
}
