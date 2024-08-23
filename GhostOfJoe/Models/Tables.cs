using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;

namespace GhostOfJoe.Models;

public class Tables {
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
    
    
    public Tables Table { get; set; } = null!;
}

public class Titles {
    [Key] 
    public int title_id { get; set; }

    [Column(TypeName = "Text")] 
    public string title { get; set; } = null!;

    [Column(TypeName = "Integer")] 
    public int user_id { get; set; }
    
    
    public Users user { get; set; } = null!;
}

public class Games {
    [Key] 
    public int game_id { get; set; }

    [Column(TypeName = "Integer")] 
    public string title { get; set; } = null!;

    [Column(TypeName = "Integer")]
    public ulong server_id { get; set; }
    
    
    public Tables Table { get; set; } = null!;
}

public class Categories {
    [Key] 
    public int category_id { get; set; }
    
    [Column(TypeName = "Integer")]
    public int game_id { get; set; }
    
    [Column(TypeName = "Text")]
    public string name { get; set; } = null!;

    [Column(TypeName = "Text")]
    public string unit { get; set; } = null!;
    
    [Column(TypeName = "Integer")]
    public bool higherBetter { get; set; }
    
    
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
    
    
    public Categories category { get; set; } = null!;

    public Users user { get; set; } = null!;
}