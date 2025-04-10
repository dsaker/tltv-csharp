using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;

namespace TalkLikeTv.Services;

public class TitleValidationService
{
    private readonly ITitleRepository _titleRepository;
    private readonly ILanguageRepository _languageRepository;

    public TitleValidationService(ITitleRepository titleRepository, ILanguageRepository languageRepository)
    {
        _titleRepository = titleRepository;
        _languageRepository = languageRepository;
    }

    public async Task<(bool IsValid, List<string> Errors)> ValidateAsync(Title title, CancellationToken cancel = default)
    {
        var errors = new List<string>();

        // Check for duplicate titles
        if (!string.IsNullOrWhiteSpace(title.TitleName))
        {
            var existingTitle = await _titleRepository.RetrieveByNameAsync(title.TitleName, cancel);
            if (existingTitle != null)
            {
                errors.Add($"A title with name '{title.TitleName}' already exists");
            }
        }

        // Check if OriginalLanguageId exists
        if (title.OriginalLanguageId.HasValue)
        {
            var languageId = title.OriginalLanguageId.Value.ToString();
            var language = await _languageRepository.RetrieveAsync(
                languageId,
                cancel);
            if (language == null)
            {
                errors.Add($"Language with ID {languageId} does not exist");
            }
        }

        return (errors.Count == 0, errors);
    }
}