using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Repositories;

public interface ITranslateRepository
{
    Task<Translate[]> RetrieveAllAsync(CancellationToken token = default);
    Task<Translate?> RetrieveAsync(string phraseId, string languageId, CancellationToken token = default);
    Task<Translate> CreateAsync(Translate translate, CancellationToken token = default);
    Task<bool> UpdateAsync(string phraseId, string translateId, Translate translate, CancellationToken token = default);
    Task<bool> DeleteAsync(string phraseId, string translateId, CancellationToken token = default);
    Task<List<Translate>> CreateManyAsync(List<Translate> translates, CancellationToken token = default);
    Task<List<Translate>> GetTranslatesByLanguageAndPhrasesAsync(int languageId, IEnumerable<int> phraseIds, CancellationToken token = default);
}