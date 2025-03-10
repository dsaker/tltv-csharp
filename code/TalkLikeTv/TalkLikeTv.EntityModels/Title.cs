using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

[Index("TitleName", Name = "UQ__Titles__252BE89C0BF9B926", IsUnique = true)]
public partial class Title
{
    [Key]
    [Column("TitleID")]
    public int TitleId { get; set; }

    [StringLength(64)]
    public string TitleName { get; set; } = null!;

    [StringLength(256)]
    public string? Description { get; set; }

    public int NumPhrases { get; set; }

    [Column("OriginalLanguageID")]
    public int? OriginalLanguageId { get; set; }

    [ForeignKey("OriginalLanguageId")]
    [InverseProperty("Titles")]
    public virtual Language? OriginalLanguage { get; set; }

    [InverseProperty("Title")]
    public virtual ICollection<Phrase> Phrases { get; set; } = new List<Phrase>();
}
