using System.ComponentModel.DataAnnotations;

namespace TalkLikeTv.WebApi.Models;

public class AudioFromTitleApiModel
{
    [Required]
    public int TitleId { get; set; }
    
    [Required]
    public int ToVoiceId { get; set; }
    
    [Required]
    public int FromVoiceId { get; set; }
    
    [Required]
    [Range(3, 10)]
    public int PauseDuration { get; set; }
    
    [Required]
    public string? Pattern { get; set; }
    
    [Required]
    public string Token { get; set; } = string.Empty;
}