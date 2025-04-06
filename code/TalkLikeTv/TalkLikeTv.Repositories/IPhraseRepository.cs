using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Repositories;

public interface IPhraseRepository
{
    Task<List<Phrase>> GetPhrasesByTitleIdAsync(int titleId, CancellationToken cancel = default);
    Task<Phrase?> RetrieveAsync(string id, CancellationToken token = default);
    Task<Phrase> CreateAsync(Phrase phrase, CancellationToken token = default);
    Task<bool> UpdateAsync(string id, Phrase phrase, CancellationToken token = default);
    Task<bool> DeleteAsync(string id, CancellationToken token = default);
    Task<List<Phrase>> CreateManyAsync(List<Phrase> phrases, CancellationToken token = default);
}