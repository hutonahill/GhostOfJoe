using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;

namespace GhostOfJoe.Models;

// just here so I can name the file Tables.cs
public class Tables { }

public class Servers {
    [Key]
    public ulong server_id { get; set; }

    [Column(TypeName = "Integer")] 
    public bool safeFlow { get; set; } = true;
}

public class Users {
    [Key] 
    public int user_id { get; set; }
    
    [Column(TypeName = "Integer")]
    public ulong discordUser_id { get; set; }
    
    [Column(TypeName = "Integer")]
    public ulong server_id { get; set; }
    
    
    [ForeignKey(nameof(server_id))]
    public Servers Server { get; set; } = null!;
}

public class Titles {
    [Key] 
    public int title_id { get; set; }

    [Column(TypeName = "Text")] 
    [StringLength(75)]
    public string title { get; set; } = null!;

    [Column(TypeName = "Integer")] 
    public int user_id { get; set; }
    
    
    [ForeignKey(nameof(user_id))]
    public Users user { get; set; } = null!;
}

public class Games {
    [Key] 
    public int game_id { get; set; }

    [Column(TypeName = "Integer")] 
    [StringLength(75)]
    public string title { get; set; } = null!;

    [Column(TypeName = "Integer")]
    public ulong server_id { get; set; }
    
    [ForeignKey(nameof(server_id))]
    public Servers Server { get; set; } = null!;
}

public class Categories {
    [Key] 
    public int category_id { get; set; }
    
    [Column(TypeName = "Integer")]
    public int game_id { get; set; }
    
    [Column(TypeName = "Text")]
    [StringLength(75)]
    public string name { get; set; } = null!;

    [Column(TypeName = "Text")]
    [StringLength(75)]
    public string unit { get; set; } = null!;
    
    [Column(TypeName = "Integer")]
    public bool higherBetter { get; set; }
    
    
    [ForeignKey(nameof(game_id))]
    public Games game { get; set; } = null!;
}

public class Scores {
    [Key] 
    public int score_id { get; set; }
    
    [Column(TypeName = "Numeric(10,1)")]
    public double value { get; set; }
    
    [Column(TypeName = "Integer")]
    public int category_id { get; set; }
    
    [Column(TypeName = "Integer")]
    public int user_id { get; set; }
    
    
    [ForeignKey(nameof(category_id))]
    public Categories category { get; set; } = null!;
    
    [ForeignKey(nameof(user_id))]
    public Users user { get; set; } = null!;
}