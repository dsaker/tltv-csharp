using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

[Index("Name", Name = "UQ_Languages_Name", IsUnique = true)]
[Index("NativeName", Name = "UQ_Languages_NativeName", IsUnique = true)]
[Index("Tag", Name = "UQ_Languages_Tag", IsUnique = true)]
public partial class Language
{
    [Key]
    public int LanguageId { get; set; }

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
    public virtual Voice? Voice { get; set; }
}
