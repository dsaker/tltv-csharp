using System.ComponentModel.DataAnnotations;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Mvc.Models.Validation;

namespace TalkLikeTv.Mvc.Models;


public record CreateTitleFormModel(
    [Required]
    Voice? ToVoice,
    [Required]
    Voice? FromVoice,
    [Required]
    [Range(3, 10)]
    int? PauseDuration,
    [Required]
    string? Pattern,
    [Required]
    [Length(26, 26)]
    string? Token,
    [Required]
    [MaxLength(64)]
    string? TitleName,
    [Required]
    int? FileLangId,
    [Required]
    [MaxLength(256)]
    string? Description,
    [Required]
    [Display(Name = "Upload File")] 
    [MaxFileSize(8192 * 8)] // Limit file size to 64 KB
    IFormFile? File
);