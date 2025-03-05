using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("bills")]
public partial class Bill
{
    [Key]
    [Column("bill_id")]
    public int BillId { get; set; }

    [Column("short_name")]
    public string ShortName { get; set; } = null!;

    [Column("description")]
    public string Description { get; set; } = null!;
}
