using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("querks")]
public partial class Querk
{
    [Key]
    [Column("querk_id")]
    public int QuerkId { get; set; }

    [Column("name")]
    public int Name { get; set; }
}
