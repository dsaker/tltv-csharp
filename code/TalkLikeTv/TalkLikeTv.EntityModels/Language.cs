using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

public partial class Language
{
    [Key]
    public int LanguageId { get; set; }

    [Column("Language")]
    [StringLength(32)]
    public string Language1 { get; set; } = null!;

    [StringLength(8)]
    public string Tag { get; set; } = null!;

    [InverseProperty("OriginalLanguage")]
    public virtual ICollection<Title> Titles { get; set; } = new List<Title>();

    [InverseProperty("Language")]
    public virtual ICollection<Translate> Translates { get; set; } = new List<Translate>();
}
