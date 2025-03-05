using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class Game
{
    public int GameId { get; set; }

    public int Title { get; set; }

    public int ServerId { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual Server Server { get; set; } = null!;
}
