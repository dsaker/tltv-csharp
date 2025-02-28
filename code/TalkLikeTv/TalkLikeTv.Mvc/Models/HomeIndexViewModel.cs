using System.ComponentModel.DataAnnotations;

namespace TalkLikeTv.Mvc.Models;

public class HomeIndexViewModel
{
    public int VisitorCount { get; set; } = 0;
    [Required]
    [Display(Name = "Upload File")]
    public IFormFile? File { get; set; }
}
