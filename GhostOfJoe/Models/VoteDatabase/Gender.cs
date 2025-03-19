using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhostOfJoe.Models.VoteDatabase;

[Table("gender")]
public partial class Gender
{
    [Column("name")]
    public int Name { get; set; }

    [Key]
    [Column("gender_id")]
    public int GenderId { get; set; }

    [InverseProperty("Gender")]
    public virtual ICollection<Representitive> Representitives { get; set; } = new List<Representitive>();
}
