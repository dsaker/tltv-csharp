using System.ComponentModel.DataAnnotations;
using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Mvc.Models;

public record AudioFromTitleFormModel(
    [Required]
    int? TitleId,
    [Required]
    int? ToVoiceId,
    [Required]
    int? FromVoiceId,
    [Required]
    [Range(3, 10)]
    int? PauseDuration,
    [Required]
    string? Pattern,
    [Required]
    [Length(26, 26)]
    string? Token
);