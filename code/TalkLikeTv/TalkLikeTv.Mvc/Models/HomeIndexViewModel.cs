using System.ComponentModel.DataAnnotations;

namespace TalkLikeTv.Mvc.Models;

public class HomeIndexViewModel
{
    [Required]
    [Display(Name = "Upload File")]
    public IFormFile? File { get; set; }
}
