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
    public double Value { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Scores")]
    public virtual Categories Categories { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Scores")]
    public virtual Users Users { get; set; } = null!;
}
