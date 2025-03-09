using System.ComponentModel.DataAnnotations;
using TalkLikeTv.Mvc.Models.Validation;

namespace TalkLikeTv.Mvc.Models;

public class HomeIndexViewModel
{
    [Required]
    [Display(Name = "Upload File")]
    [MaxFileSize(8192 * 8)]
    public IFormFile? File { get; set; }
}
