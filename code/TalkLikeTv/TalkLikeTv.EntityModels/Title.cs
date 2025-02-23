using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

[Index("Title1", Name = "UQ__Titles__2CB664DC9F414B11", IsUnique = true)]
public partial class Title
{
    [Key]
    [Column("TitleID")]
    public int TitleId { get; set; }

    [Column("Title")]
    [StringLength(64)]
    public string Title1 { get; set; } = null!;

    public int NumPhrases { get; set; }

    [Column("OriginalLanguageID")]
    public int? OriginalLanguageId { get; set; }

    [ForeignKey("OriginalLanguageId")]
    [InverseProperty("Titles")]
    public virtual Language? OriginalLanguage { get; set; }

    [InverseProperty("Title")]
    public virtual ICollection<Phrase> Phrases { get; set; } = new List<Phrase>();
}
