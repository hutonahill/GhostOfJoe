using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class Bill
{
    public int BillId { get; set; }

    public string ShortName { get; set; } = null!;

    public string Description { get; set; } = null!;
}
