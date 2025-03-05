using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class Server
{
    public int ServerId { get; set; }

    public int SafeFlow { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
