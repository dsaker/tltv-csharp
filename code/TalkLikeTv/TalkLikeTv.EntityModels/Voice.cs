using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

[Index("ShortName", Name = "UQ__Voices__A6160FD116C784E7", IsUnique = true)]
public partial class Voice
{
    [Key]
    [Column("VoiceID")]
    public int VoiceId { get; set; }

    [Column("LanguageID")]
    public int? LanguageId { get; set; }

    [StringLength(32)]
    public string DisplayName { get; set; } = null!;

    [StringLength(32)]
    public string LocalName { get; set; } = null!;

    [StringLength(64)]
    public string ShortName { get; set; } = null!;

    [StringLength(8)]
    public string Gender { get; set; } = null!;

    [StringLength(16)]
    public string Locale { get; set; } = null!;

    [StringLength(32)]
    public string LocaleName { get; set; } = null!;

    public int SampleRateHertz { get; set; }

    [StringLength(8)]
    public string VoiceType { get; set; } = null!;

    [StringLength(32)]
    public string Status { get; set; } = null!;

    public int WordsPerMinute { get; set; }

    [ForeignKey("LanguageId")]
    [InverseProperty("Voices")]
    public virtual Language? Language { get; set; }

    [ForeignKey("VoiceId")]
    [InverseProperty("Voices")]
    public virtual ICollection<Personality> Personalities { get; set; } = new List<Personality>();

    [ForeignKey("VoiceId")]
    [InverseProperty("Voices")]
    public virtual ICollection<Scenario> Scenarios { get; set; } = new List<Scenario>();

    [ForeignKey("VoiceId")]
    [InverseProperty("Voices")]
    public virtual ICollection<Style> Styles { get; set; } = new List<Style>();
}
