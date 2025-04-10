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

    [Required(ErrorMessage = "Title name is required")]
    [StringLength(64, ErrorMessage = "Title name cannot exceed 64 characters")]
    public string TitleName { get; set; } = null!;

    [StringLength(256, ErrorMessage = "Description cannot exceed 256 characters")]
    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Number of phrases must be non-negative")]
    public int NumPhrases { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Popularity must be non-negative")]
    public int Popularity { get; set; } = 0;

    [Column("OriginalLanguageID")]
    public int? OriginalLanguageId { get; set; }

    [ForeignKey("OriginalLanguageId")]
    [InverseProperty("Titles")]
    public virtual Language? OriginalLanguage { get; set; }

    [InverseProperty("Title")]
    public virtual ICollection<Phrase> Phrases { get; set; } = new List<Phrase>();
}
