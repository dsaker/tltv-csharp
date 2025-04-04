using TalkLikeTv.EntityModels; // To use Language.

namespace TalkLikeTv.Repositories;

public interface ILanguageRepository
{
    Task<Language[]> RetrieveAllAsync();
    Task<Language?> RetrieveAsync(string id,
        CancellationToken token);
    Task<Language?> RetrieveByTagAsync(string code,
        CancellationToken token);
}