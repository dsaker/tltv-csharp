using System.ComponentModel.DataAnnotations;

namespace TalkLikeTv.WebApi.Models;

// Model for the API endpoint
public class CreateTitleFromFileApiModel
{
    [Required]
    public int ToVoiceId { get; set; }

    [Required]
    public int FromVoiceId { get; set; }

    [Required]
    public int PauseDuration { get; set; }

    [Required]
    public string Pattern { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    public string TitleName { get; set; }

    public string? Description { get; set; }

    [Required]
    public IFormFile File { get; set; }
}