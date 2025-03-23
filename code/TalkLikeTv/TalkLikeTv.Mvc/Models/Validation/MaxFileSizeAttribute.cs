using System.ComponentModel.DataAnnotations;

namespace TalkLikeTv.Mvc.Models.Validation;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSize;
    public MaxFileSizeAttribute(int maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IFormFile file)
        {
            return new ValidationResult("Invalid file type.");
        }

        if (file.Length > _maxFileSize)
        {
            return new ValidationResult($"Maximum allowed file size is {_maxFileSize} bytes.");
        }

        return ValidationResult.Success!;
    }
}