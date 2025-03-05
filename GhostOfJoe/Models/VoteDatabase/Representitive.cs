using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GhostOfJoe.Models;

[Table("representitives")]
public partial class Representitive
{
    [Key]
    [Column("rep_id")]
    public int RepId { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = null!;

    [Column("last_name")]
    public string LastName { get; set; } = null!;

    [Column("phonetic")]
    public string Phonetic { get; set; } = null!;

    [Column("ipa")]
    public string Ipa { get; set; } = null!;

    [Column("code")]
    public string Code { get; set; } = null!;

    [Column("first_elected")]
    public string FirstElected { get; set; } = null!;

    [Column("birth_year")]
    public int BirthYear { get; set; }

    [Column("lgbt")]
    public int Lgbt { get; set; }

    [Column("race_id")]
    public int RaceId { get; set; }

    [Column("office_id")]
    public int OfficeId { get; set; }

    [Column("party_id")]
    public int PartyId { get; set; }

    [Column("gender_id")]
    public int GenderId { get; set; }

    [Column("religon_id")]
    public int ReligonId { get; set; }

    [ForeignKey("GenderId")]
    [InverseProperty("Representitives")]
    public virtual Gender Gender { get; set; } = null!;

    [ForeignKey("OfficeId")]
    [InverseProperty("Representitives")]
    public virtual Office Office { get; set; } = null!;

    [ForeignKey("PartyId")]
    [InverseProperty("Representitives")]
    public virtual Party Party { get; set; } = null!;

    [ForeignKey("RaceId")]
    [InverseProperty("Representitives")]
    public virtual Race Race { get; set; } = null!;

    [ForeignKey("ReligonId")]
    [InverseProperty("Representitives")]
    public virtual Religon Religon { get; set; } = null!;
}
