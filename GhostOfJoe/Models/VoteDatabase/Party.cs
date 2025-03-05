using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("party")]
public partial class Party
{
    [Key]
    [Column("party_id")]
    public int PartyId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [InverseProperty("Party")]
    public virtual ICollection<Representitive> Representitives { get; set; } = new List<Representitive>();
}
