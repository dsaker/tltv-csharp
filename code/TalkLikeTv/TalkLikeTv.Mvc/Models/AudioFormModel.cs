using System.ComponentModel.DataAnnotations;

namespace TalkLikeTv.Mvc.Models;

public record AudioFormModel(
    [Required] int FromVoice,
    [Required] int ToVoice,
    [Required] int PauseDuration,
    [Required] int Pattern);