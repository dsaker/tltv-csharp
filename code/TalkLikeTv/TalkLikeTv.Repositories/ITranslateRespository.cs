using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Repositories;

public interface ITranslateRepository
{
    Task<List<Translate>> CreateManyAsync(List<Translate> translates, CancellationToken token = default);
    Task<List<Translate>> GetTranslatesByLanguageAndPhrasesAsync(int languageId, IEnumerable<int> phraseIds, CancellationToken token = default);
}