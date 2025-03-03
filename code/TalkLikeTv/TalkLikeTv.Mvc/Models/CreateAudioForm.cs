using System.ComponentModel.DataAnnotations;

namespace TalkLikeTv.Mvc.Models;

public class CreateAudioForm
{
    [Required]
    public string ToVoice { get; set; }
    [Required]
    public string FromVoice { get; set; }
    [Range(3,10)]
    [Required]
    public string PauseDuration { get; set; }
    [Range(1,3)]
    [Required]
    public string Pattern { get; set; }
}