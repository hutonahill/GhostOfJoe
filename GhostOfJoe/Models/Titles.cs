using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("titles")]
public partial class Titles
{
    [Key]
    [Column("title_id")]
    public int TitleId { get; set; }

    [Column("title")]
    public string Title { get; set; } = null!;

    [Column("user_id")]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Titles")]
    public virtual Users Users { get; set; } = null!;
}
