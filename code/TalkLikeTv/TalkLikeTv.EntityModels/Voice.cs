using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

[Index("DisplayName", Name = "UQ_Voices_DisplayName", IsUnique = true)]
[Index("ShortName", Name = "UQ_Voices_ShortName", IsUnique = true)]
public partial class Voice
{
    public int VoiceId { get; set; }

    [Key]
    public int LanguageId { get; set; }

    [StringLength(32)]
    public string DisplayName { get; set; } = null!;

    [StringLength(32)]
    public string LocalName { get; set; } = null!;

    [StringLength(32)]
    public string ShortName { get; set; } = null!;

    [StringLength(8)]
    public string Gender { get; set; } = null!;

    [StringLength(8)]
    public string Locale { get; set; } = null!;

    [StringLength(32)]
    public string LocaleName { get; set; } = null!;

    public int SampleRateHertz { get; set; }

    [StringLength(8)]
    public string VoiceType { get; set; } = null!;

    [StringLength(8)]
    public string Status { get; set; } = null!;

    public int WordsPerMinute { get; set; }

    [ForeignKey("LanguageId")]
    [InverseProperty("Voice")]
    public virtual Language Language { get; set; } = null!;
}
