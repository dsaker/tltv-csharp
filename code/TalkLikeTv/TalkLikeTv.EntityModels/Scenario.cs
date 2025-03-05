using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TalkLikeTv.EntityModels;

[Index("ScenarioName", Name = "UQ__Scenario__ADC5B11D0DEA5189", IsUnique = true)]
public partial class Scenario
{
    [Key]
    [Column("ScenarioID")]
    public int ScenarioId { get; set; }

    [StringLength(32)]
    public string ScenarioName { get; set; } = null!;

    [ForeignKey("ScenarioId")]
    [InverseProperty("Scenarios")]
    public virtual ICollection<Voice> Voices { get; set; } = new List<Voice>();
}
