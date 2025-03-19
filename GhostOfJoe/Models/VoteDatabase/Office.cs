using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhostOfJoe.Models.VoteDatabase;

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
