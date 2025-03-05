using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("servers")]
public partial class Servers
{
    [Key]
    [Column("server_id")]
    public ulong ServerId { get; set; }

    [Column("safeFlow")]
    public bool SafeFlow { get; set; }

    [InverseProperty("Server")]
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();

    [InverseProperty("Server")]
    public virtual ICollection<Users> Users { get; set; } = new List<Users>();
}
