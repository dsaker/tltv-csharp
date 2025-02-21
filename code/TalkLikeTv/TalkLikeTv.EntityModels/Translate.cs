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
    public int PhraseId { get; set; }

    [Key]
    public int LanguageId { get; set; }

    [StringLength(128)]
    public string? Phrase { get; set; }

    [Column("phrase_hint")]
    [StringLength(128)]
    public string? PhraseHint { get; set; }

    [ForeignKey("LanguageId")]
    [InverseProperty("Translates")]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("PhraseId")]
    [InverseProperty("Translates")]
    public virtual Phrase PhraseNavigation { get; set; } = null!;
}
