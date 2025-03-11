using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

[Index("Name", Name = "UQ__Language__737584F60620E8D3", IsUnique = true)]
[Index("Tag", Name = "UQ__Language__C4516413E29C3320", IsUnique = true)]
public partial class Language
{
    [Key]
    [Column("LanguageID")]
    public int LanguageId { get; set; }

    [StringLength(16)]
    public string Platform { get; set; } = null!;

    [StringLength(32)]
    public string Name { get; set; } = null!;

    [StringLength(32)]
    public string NativeName { get; set; } = null!;

    [StringLength(8)]
    public string Tag { get; set; } = null!;

    [InverseProperty("OriginalLanguage")]
    public virtual ICollection<Title> Titles { get; set; } = new List<Title>();

    [InverseProperty("Language")]
    public virtual ICollection<Translate> Translates { get; set; } = new List<Translate>();

    [InverseProperty("Language")]
    public virtual ICollection<Voice> Voices { get; set; } = new List<Voice>();
}
