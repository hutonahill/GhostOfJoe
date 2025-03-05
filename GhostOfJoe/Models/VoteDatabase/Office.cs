using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("office")]
public partial class Office
{
    [Key]
    [Column("office_id")]
    public int OfficeId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [InverseProperty("Office")]
    public virtual ICollection<Representitive> Representitives { get; set; } = new List<Representitive>();
}
