using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("servers")]
public partial class Server
{
    [Key]
    [Column("server_id")]
    public int ServerId { get; set; }

    [Column("safeFlow")]
    public int SafeFlow { get; set; }

    [InverseProperty("Server")]
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();

    [InverseProperty("Server")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
