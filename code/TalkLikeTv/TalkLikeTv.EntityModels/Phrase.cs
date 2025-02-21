using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

public partial class Phrase
{
    [Key]
    public int PhraseId { get; set; }

    public int TitleId { get; set; }

    [ForeignKey("TitleId")]
    [InverseProperty("Phrases")]
    public virtual Title Title { get; set; } = null!;

    [InverseProperty("PhraseNavigation")]
    public virtual ICollection<Translate> Translates { get; set; } = new List<Translate>();
}
