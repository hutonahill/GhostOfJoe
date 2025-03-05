using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("scores")]
public partial class Score
{
    [Key]
    [Column("score_id")]
    public int ScoreId { get; set; }

    [Column("value", TypeName = "NUMERIC (10, 1)")]
    public byte[] Value { get; set; } = null!;

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Scores")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Scores")]
    public virtual User User { get; set; } = null!;
}
