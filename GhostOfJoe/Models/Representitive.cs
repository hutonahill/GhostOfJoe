using System;
using System.Collections.Generic;

namespace GhostOfJoe.Models;

public partial class Representitive
{
    public int RepId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Phonetic { get; set; } = null!;

    public string Ipa { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string FirstElected { get; set; } = null!;

    public int BirthYear { get; set; }

    public int Lgbt { get; set; }

    public int RaceId { get; set; }

    public int OfficeId { get; set; }

    public int PartyId { get; set; }

    public int GenderId { get; set; }

    public int ReligonId { get; set; }

    public virtual Gender Gender { get; set; } = null!;

    public virtual Office Office { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;

    public virtual Race Race { get; set; } = null!;

    public virtual Religon Religon { get; set; } = null!;
}
