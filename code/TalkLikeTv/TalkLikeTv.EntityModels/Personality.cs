using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

[Index("PersonalityName", Name = "UQ__Personal__6DD1E173FA88CDD2", IsUnique = true)]
public partial class Personality
{
    [Key]
    [Column("PersonalityID")]
    public int PersonalityId { get; set; }

    [StringLength(32)]
    public string PersonalityName { get; set; } = null!;

    [ForeignKey("PersonalityId")]
    [InverseProperty("Personalities")]
    public virtual ICollection<Voice> Voices { get; set; } = new List<Voice>();
}
