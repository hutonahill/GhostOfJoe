using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("religon")]
public partial class Religon
{
    [Key]
    [Column("religon_id")]
    public int ReligonId { get; set; }

    [Column("name")]
    public int? Name { get; set; }

    [InverseProperty("Religon")]
    public virtual ICollection<Representitive> Representitives { get; set; } = new List<Representitive>();
}
