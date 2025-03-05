using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

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

    [ForeignKey("GameId")]
    [InverseProperty("Categories")]
    public virtual Game Game { get; set; } = null!;

    [InverseProperty("Category")]
    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
}
