using System.ComponentModel.DataAnnotations;
using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Mvc.Models;

public record VoiceViewModel(
    int Id,
    string DisplayName,
    string LocaleName,
    string ShortName,
    string Details);

public class CreateAudioViewModel(IEnumerable<Language>? languages)
{
    // public CreateAudioInputsModel? CreateAudioInputsModel { get; set; }
    // public CreateAudioForm? CreateAudioForm { get; set; }
    public IEnumerable<Language>? Languages = languages;
    
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