using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

[Index("StyleName", Name = "UQ__Styles__23564EE6BEBA951E", IsUnique = true)]
public partial class Style
{
    [Key]
    [Column("StyleID")]
    public int StyleId { get; set; }

    [StringLength(32)]
    public string StyleName { get; set; } = null!;

    [ForeignKey("StyleId")]
    [InverseProperty("Styles")]
    public virtual ICollection<Voice> Voices { get; set; } = new List<Voice>();
}
