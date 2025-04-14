using System.ComponentModel.DataAnnotations;

namespace TalkLikeTv.WebApi.Models;

// Model for the API endpoint
public class CreateTitleFromFileApiModel
{

    [Required]
    public required string Token { get; set; }

    [Required]
    public required string TitleName { get; set; }

    public string? Description { get; set; }

    [Required]
    public required IFormFile File { get; set; }
}