using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

public partial class Token
{
    [Key]
    [Column("TokenID")]
    public int TokenId { get; set; }

    [StringLength(64)]
    public string Hash { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime Created { get; set; }

    public bool Used { get; set; }
}
