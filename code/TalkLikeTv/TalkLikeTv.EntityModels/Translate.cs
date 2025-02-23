using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

[PrimaryKey("PhraseId", "LanguageId")]
public partial class Translate
{
    [Key]
    [Column("PhraseID")]
    public int PhraseId { get; set; }

    [Key]
    [Column("LanguageID")]
    public int LanguageId { get; set; }

    [StringLength(128)]
    public string Phrase { get; set; } = null!;

    [StringLength(128)]
    public string PhraseHint { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("Translates")]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("PhraseId")]
    [InverseProperty("Translates")]
    public virtual Phrase PhraseNavigation { get; set; } = null!;
}
