using System.ComponentModel.DataAnnotations;

namespace TalkLikeTv.Mvc.Models;

public record ChooseAudioFormModel(
    int? TitleId,
    [Required] int? FromVoice,
    [Required] int? ToVoice,
    [Required] int? PauseDuration,
    [Required] string? Pattern);